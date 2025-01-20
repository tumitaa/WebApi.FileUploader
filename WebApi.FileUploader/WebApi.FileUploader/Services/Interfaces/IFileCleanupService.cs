namespace WebApi.FileUploader.Services.Interfaces;

/// <summary>
/// Service responsible for cleaning up temporary files and directories
/// </summary>
public interface IFileCleanupService
{
    /// <summary>
    /// Cleans up old files and directories based on configured retention period
    /// </summary>
    /// <returns>Number of items cleaned up</returns>
    Task<(int FilesDeleted, int DirectoriesDeleted)> CleanupOldFilesAsync();
} 