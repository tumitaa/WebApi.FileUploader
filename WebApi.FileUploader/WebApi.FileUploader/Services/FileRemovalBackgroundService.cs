using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Services;

public class FileRemovalBackgroundService
{
    private readonly IMongoDbService _mongoDbService;
    private readonly IDriveService _driveService;
    private readonly ILogger<FileRemovalBackgroundService> _logger;

    public FileRemovalBackgroundService(
        IMongoDbService mongoDbService,
        IDriveService driveService,
        ILogger<FileRemovalBackgroundService> logger)
    {
        _mongoDbService = mongoDbService;
        _driveService = driveService;
        _logger = logger;
    }

    public async Task ProcessFilesToRemoveAsync()
    {
        try
        {
            _logger.LogInformation("Starting file removal process");
            var filesToRemove = await _mongoDbService.GetFilesToRemoveAsync();

            foreach (var file in filesToRemove)
            {
                try
                {
                    _logger.LogInformation("Processing file removal: Client={Client}, Resource={Resource}, ResourceId={ResourceId}",
                        file.Client, file.Resource, file.ResourceId);

                    // Split the path into prefix and filename
                    var prefix = $"{file.Client}/{file.Resource}";
                    var fileName = file.ResourceId;
                    var bucketName = string.Empty; // Use default bucket

                    var deleted = await _driveService.DeleteFile(prefix, fileName, bucketName);
                    if (deleted)
                    {
                        await _mongoDbService.DeleteFileToRemoveAsync(file.Id);
                        _logger.LogInformation("Successfully deleted file and removed record: {Prefix}/{FileName}", prefix, fileName);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete file from S3: {Prefix}/{FileName}", prefix, fileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file removal for ID: {Id}", file.Id);
                }
            }

            _logger.LogInformation("Completed file removal process");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in file removal background service");
            throw;
        }
    }
}