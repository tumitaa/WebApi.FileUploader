using Amazon.S3.Model;

namespace WebApi.FileUploader.Services.Interfaces
{
    public interface IDriveService
    {
        Task<string> DownloadFileContent(string prefix, string fileName, string? bucketName = null);
        Task<string> DownloadHttpFile(string url);
        Task<List<S3FileResponse>> DownloadMultiFilesContent(string prefix, string? bucketName = null);
        Task<bool> FileExists(string url);
        Task<PutObjectResponse> UploadFile(string prefix, string fileName, string fileContent, string? bucketName = null);
        Task<bool> DeleteFile(string prefix, string fileName, string? bucketName = null);
    }
}