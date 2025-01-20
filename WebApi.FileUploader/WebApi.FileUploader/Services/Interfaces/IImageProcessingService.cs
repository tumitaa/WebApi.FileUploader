namespace WebApi.FileUploader.Services.Interfaces
{
    public interface IImageProcessingService
    {
        Task<MemoryStream> OptimizeImageAsync(MemoryStream originalImage, string extension, long maxSizeInBytes);
    }
} 