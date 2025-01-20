namespace WebApi.FileUploader.Infrastructure.Dtos
{
    public record FileUploadRequest(string Client, string Resource, string IdResource, string Filename, MemoryStream Content);

    public record FileUploadResponse(string FileName, string Base64, int Length);

    public record FileLocation(string Client, string Resource, string IdResource, string Filename)
    {
        public bool IsTemporary => IdResource.StartsWith("temp_", StringComparison.InvariantCultureIgnoreCase);
        public string GetPrefix() => $"{Client}/{Resource}/{IdResource}";
    }

    public class FileUploadSettings
    {
        public long MaxFileSizeInBytes { get; set; } = 5 * 1024 * 1024; // 5MB
        public string TempFilesPath { get; set; }
        public int InitialImageQuality { get; set; } = 90;
        public int MinImageQuality { get; set; } = 10;
        public int QualityReductionStep { get; set; } = 10;
    }
}