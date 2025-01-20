using WebApi.FileUploader.Infrastructure.Dtos;

namespace WebApi.FileUploader.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<FileUploadResponse> StoreFileAsync(FileLocation location, MemoryStream content);
        Task<FileUploadResponse> GetFileAsync(FileLocation location);
        Task<List<FileUploadResponse>> GetFilesAsync(FileLocation location);
        Task<bool> DeleteFileAsync(FileLocation location);
        Task<bool> MoveFilesAsync(FileLocation source, FileLocation destination);
    }
}