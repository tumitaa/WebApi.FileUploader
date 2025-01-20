using WebApi.FileUploader.Infrastructure.Dtos;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly FileUploadSettings _settings;

        public FileUploadService(
            IFileStorageService fileStorageService,
            IImageProcessingService imageProcessingService,
            FileUploadSettings settings)
        {
            _fileStorageService = fileStorageService;
            _imageProcessingService = imageProcessingService;
            _settings = settings;
        }

        public async Task<FileUploadResponse> UploadFile(string client, string resource, string idresource, string filename, MemoryStream content)
        {
            var location = new FileLocation(client, resource, idresource, filename);
            var ext = Path.GetExtension(filename).ToLowerInvariant();

            if (!filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && content.Length > _settings.MaxFileSizeInBytes)
            {
                var optimizedImage = await _imageProcessingService.OptimizeImageAsync(content, ext, _settings.MaxFileSizeInBytes);
                if (optimizedImage == null)
                    throw new InvalidOperationException("Failed to process image");

                content = optimizedImage;
            }

            return await _fileStorageService.StoreFileAsync(location, content);
        }

        public async Task<List<FileUploadResponse>> GetFiles(string client, string resource, string idresource, string filename = null)
        {
            var location = new FileLocation(client, resource, idresource, filename);
            return await _fileStorageService.GetFilesAsync(location);
        }

        public async Task<FileUploadResponse> DownloadFile(string client, string resource, string idresource, string filename)
        {
            var location = new FileLocation(client, resource, idresource, filename);
            return await _fileStorageService.GetFileAsync(location);
        }

        public async Task<bool> ApplyTempFiles(string client, string resource, string idresource, string newidresource)
        {
            var source = new FileLocation(client, resource, idresource, null);
            var destination = new FileLocation(client, resource, newidresource, null);
            return await _fileStorageService.MoveFilesAsync(source, destination);
        }

        public async Task<bool> DeleteFile(string client, string resource, string idresource, string filename)
        {
            var location = new FileLocation(client, resource, idresource, filename);
            return await _fileStorageService.DeleteFileAsync(location);
        }
    }

    public record FileUploadResponse(string FileName, string Base64, int Length);
}
