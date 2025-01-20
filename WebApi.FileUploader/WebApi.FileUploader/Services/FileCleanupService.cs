using Microsoft.Extensions.Options;
using WebApi.FileUploader.Infrastructure.Configuration;
using WebApi.FileUploader.Infrastructure.Exceptions;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Services;

/// <summary>
/// Service responsible for cleaning up temporary files and directories
/// </summary>
public class FileCleanupService : IFileCleanupService
{
    private readonly string _tempFilesPath;
    private readonly int _retentionHours;
    private readonly ILogger<FileCleanupService> _logger;

    public FileCleanupService(
        IOptions<FileCleanupOptions> options,
        ILogger<FileCleanupService> logger)
    {
        if (string.IsNullOrEmpty(options.Value.TempFilesPath))
        {
            throw new ArgumentException("TempFilesPath is not configured.");
        }

        _tempFilesPath = options.Value.TempFilesPath;
        _retentionHours = options.Value.RetentionHours;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<(int FilesDeleted, int DirectoriesDeleted)> CleanupOldFilesAsync()
    {
        if (!Directory.Exists(_tempFilesPath))
        {
            _logger.LogWarning("Temporary files path does not exist: {TempFilesPath}", _tempFilesPath);
            return (0, 0);
        }

        try
        {
            var now = DateTime.UtcNow;
            var cutoffTime = now.AddHours(-_retentionHours);

            var filesDeleted = await CleanupFilesAsync(cutoffTime);
            var directoriesDeleted = await CleanupDirectoriesAsync(cutoffTime);

            _logger.LogInformation(
                "Cleanup completed. Deleted {FilesCount} files and {DirectoriesCount} directories",
                filesDeleted,
                directoriesDeleted);

            return (filesDeleted, directoriesDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during file cleanup operation");
            throw new FileProcessingException("Failed to clean up temporary files", ex);
        }
    }

    private async Task<int> CleanupFilesAsync(DateTime cutoffTime)
    {
        var deletedCount = 0;
        var files = Directory.GetFiles(_tempFilesPath);

        foreach (var file in files)
        {
            if (await ShouldDeleteFileAsync(file, cutoffTime))
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                    _logger.LogDebug("Deleted file: {FilePath}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file: {FilePath}", file);
                }
            }
        }

        return deletedCount;
    }

    private async Task<int> CleanupDirectoriesAsync(DateTime cutoffTime)
    {
        var deletedCount = 0;
        var directories = Directory.GetDirectories(_tempFilesPath);

        foreach (var directory in directories)
        {
            if (await ShouldDeleteDirectoryAsync(directory, cutoffTime))
            {
                try
                {
                    Directory.Delete(directory, true);
                    deletedCount++;
                    _logger.LogDebug("Deleted directory: {DirectoryPath}", directory);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete directory: {DirectoryPath}", directory);
                }
            }
        }

        return deletedCount;
    }

    private Task<bool> ShouldDeleteFileAsync(string filePath, DateTime cutoffTime)
    {
        var fileInfo = new FileInfo(filePath);
        return Task.FromResult(fileInfo.LastWriteTimeUtc < cutoffTime);
    }

    private Task<bool> ShouldDeleteDirectoryAsync(string directoryPath, DateTime cutoffTime)
    {
        var dirInfo = new DirectoryInfo(directoryPath);
        return Task.FromResult(dirInfo.LastWriteTimeUtc < cutoffTime);
    }
} 