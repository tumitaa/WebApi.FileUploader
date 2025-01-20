using WebApi.FileUploader.Infrastructure.Dtos;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IDriveService _driveService;
        private readonly FileUploadSettings _settings;

        public FileStorageService(IDriveService driveService, FileUploadSettings settings)
        {
            _driveService = driveService;
            _settings = settings;
        }

        public async Task<FileUploadResponse> StoreFileAsync(FileLocation location, MemoryStream content)
        {
            string base64 = Convert.ToBase64String(content.ToArray());

            if (location.IsTemporary)
            {
                await SaveToTempFolder(location, content);
            }
            else
            {
                await _driveService.UploadFile(location.GetPrefix(), location.Filename, base64);
            }

            return new FileUploadResponse(location.Filename, base64, base64.Length);
        }

        public async Task<FileUploadResponse> GetFileAsync(FileLocation location)
        {
            if (location.IsTemporary)
            {
                string tempFilePath = GetTempFilePath(location);
                if (!File.Exists(tempFilePath))
                    return null;

                var fileBytes = await File.ReadAllBytesAsync(tempFilePath);
                var base64 = Convert.ToBase64String(fileBytes);
                return new FileUploadResponse(location.Filename, base64, base64.Length);
            }

            var result = await _driveService.DownloadFileContent(location.GetPrefix(), location.Filename);
            return result == null ? null : new FileUploadResponse(location.Filename, result, result.Length);
        }

        public async Task<List<FileUploadResponse>> GetFilesAsync(FileLocation location)
        {
            if (location.IsTemporary)
            {
                if (!string.IsNullOrEmpty(location.Filename))
                {
                    var file = await GetFileAsync(location);
                    return file != null ? new List<FileUploadResponse> { file } : null;
                }

                var filesPath = Path.Combine(_settings.TempFilesPath, location.IdResource);
                Directory.CreateDirectory(filesPath);
                var files = Directory.GetFiles(filesPath);

                return files.Select(file =>
                {
                    var fileBytes = File.ReadAllBytes(file);
                    var base64 = Convert.ToBase64String(fileBytes);
                    return new FileUploadResponse(Path.GetFileName(file), base64, base64.Length);
                }).ToList();
            }

            if (!string.IsNullOrEmpty(location.Filename))
            {
                var result = await _driveService.DownloadFileContent(location.GetPrefix(), location.Filename);
                return new List<FileUploadResponse> { new FileUploadResponse(location.Filename, result, result.Length) };
            }

            var results = await _driveService.DownloadMultiFilesContent(location.GetPrefix());
            return results.Select(x => new FileUploadResponse(x.name, x.base64, x.base64.Length)).ToList();
        }

        public async Task<bool> DeleteFileAsync(FileLocation location)
        {
            if (location.IsTemporary)
            {
                string tempFilePath = GetTempFilePath(location);
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                    return true;
                }
                return false;
            }

            return await _driveService.DeleteFile(location.GetPrefix(), location.Filename);
        }

        public async Task<bool> MoveFilesAsync(FileLocation source, FileLocation destination)
        {
            if (!source.IsTemporary)
                throw new InvalidOperationException("Source must be a temporary location");

            string tempFolderPath = Path.Combine(_settings.TempFilesPath, source.IdResource);
            if (!Directory.Exists(tempFolderPath))
                throw new DirectoryNotFoundException("Temporary folder does not exist");

            var files = Directory.GetFiles(tempFolderPath);
            if (files.Length == 0)
                throw new InvalidOperationException("No files found in the temporary folder");

            foreach (var filePath in files)
            {
                string filename = Path.GetFileName(filePath);
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                string base64 = Convert.ToBase64String(fileBytes);

                await _driveService.UploadFile(destination.GetPrefix(), filename, base64);
                File.Delete(filePath);
            }

            if (Directory.GetFiles(tempFolderPath).Length == 0)
            {
                Directory.Delete(tempFolderPath);
            }

            return true;
        }

        private async Task SaveToTempFolder(FileLocation location, MemoryStream content)
        {
            string tempFilePath = GetTempFilePath(location);
            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

            await using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write);
            content.Position = 0;
            await content.CopyToAsync(fileStream);
        }

        private string GetTempFilePath(FileLocation location)
        {
            if (string.IsNullOrEmpty(_settings.TempFilesPath))
                throw new InvalidOperationException("TempFilesPath is not configured");

            return Path.Combine(_settings.TempFilesPath, location.IdResource, location.Filename);
        }
    }
}