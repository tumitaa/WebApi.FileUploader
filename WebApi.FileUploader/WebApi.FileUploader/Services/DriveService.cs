using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System.Text;
using WebApi.FileUploader.Infrastructure.Configuration;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Services;

public class DriveService : IDriveService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly S3Options _s3Options;
    private readonly ILogger<DriveService> _logger;

    public DriveService(
        IAmazonS3 s3Client,
        IHttpClientFactory httpClientFactory,
        IOptions<S3Options> s3Options,
        ILogger<DriveService> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _s3Options = s3Options?.Value ?? throw new ArgumentNullException(nameof(s3Options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> DownloadFileContent(string prefix, string fileName, string? bucketName = null)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(fileName);

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName ?? _s3Options.BucketName,
                Key = $"{prefix}/{fileName}",
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            using var reader = new StreamReader(responseStream, Encoding.UTF8);

            return response.HttpStatusCode == System.Net.HttpStatusCode.OK
                ? await reader.ReadToEndAsync()
                : null;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to download file content from S3. Prefix: {Prefix}, FileName: {FileName}, BucketName: {BucketName}",
                prefix, fileName, bucketName ?? _s3Options.BucketName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error downloading file content from S3. Prefix: {Prefix}, FileName: {FileName}, BucketName: {BucketName}",
                prefix, fileName, bucketName ?? _s3Options.BucketName);
            return null;
        }
    }

    public async Task<string> DownloadHttpFile(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        try
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            _logger.LogWarning("Failed to download HTTP file. URL: {Url}, StatusCode: {StatusCode}",
                url, response.StatusCode);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while downloading file. URL: {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error downloading HTTP file. URL: {Url}", url);
            return null;
        }
    }

    public async Task<List<S3FileResponse>> DownloadMultiFilesContent(string prefix, string? bucketName = null)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        try
        {
            var files = new List<S3FileResponse>();
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName ?? _s3Options.BucketName,
                Prefix = prefix
            };

            ListObjectsV2Response response;
            do
            {
                response = await _s3Client.ListObjectsV2Async(request);

                foreach (var entry in response.S3Objects)
                {
                    var content = await GetObjectContent(entry.Key, bucketName);
                    if (content != null)
                    {
                        files.Add(new S3FileResponse(entry.Key, content));
                    }
                }

                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);

            return files;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to download multiple files from S3. Prefix: {Prefix}, BucketName: {BucketName}",
                prefix, bucketName ?? _s3Options.BucketName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error downloading multiple files from S3. Prefix: {Prefix}, BucketName: {BucketName}",
                prefix, bucketName ?? _s3Options.BucketName);
            return null;
        }
    }

    public async Task<bool> FileExists(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        try
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if file exists. URL: {Url}", url);
            return false;
        }
    }

    public async Task<PutObjectResponse> UploadFile(string prefix, string fileName, string fileContent, string? bucketName = null)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(fileContent);

        prefix ??= "default";

        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName ?? _s3Options.BucketName,
                Key = $"{prefix}/{fileName}",
                ContentBody = fileContent,
            };

            return await _s3Client.PutObjectAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to S3. Prefix: {Prefix}, FileName: {FileName}, BucketName: {BucketName}",
                prefix, fileName, bucketName ?? _s3Options.BucketName);
            throw;
        }
    }

    public async Task<bool> DeleteFile(string prefix, string fileName, string? bucketName = null)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(fileName);

        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName ?? _s3Options.BucketName,
                Key = $"{prefix}/{fileName}"
            };

            var response = await _s3Client.DeleteObjectAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from S3. Prefix: {Prefix}, FileName: {FileName}, BucketName: {BucketName}",
                prefix, fileName, bucketName ?? _s3Options.BucketName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting file from S3. Prefix: {Prefix}, FileName: {FileName}, BucketName: {BucketName}",
                prefix, fileName, bucketName ?? _s3Options.BucketName);
            return false;
        }
    }

    private async Task<string> GetObjectContent(string key, string? bucketName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName ?? _s3Options.BucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var stream = response.ResponseStream;
            using var reader = new StreamReader(stream, Encoding.UTF8);

            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get object content from S3. Key: {Key}, BucketName: {BucketName}",
                key, bucketName ?? _s3Options.BucketName);
            return null;
        }
    }
}

public record S3FileResponse(string name, string base64);