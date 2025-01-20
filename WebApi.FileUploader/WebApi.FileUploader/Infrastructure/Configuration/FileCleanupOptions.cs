namespace WebApi.FileUploader.Infrastructure.Configuration;

public class FileCleanupOptions
{
    public const string SectionName = "FileCleanup";
    
    public string TempFilesPath { get; set; }
    public int RetentionHours { get; set; } = 2; // Default to 2 hours
} 