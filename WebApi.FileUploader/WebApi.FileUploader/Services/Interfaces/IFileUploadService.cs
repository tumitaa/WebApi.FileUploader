using WebApi.FileUploader.Infrastructure.Dtos;

namespace WebApi.FileUploader.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<FileUploadResponse> UploadFile(string client, string resource, string idresource, string filename, MemoryStream content);
        Task<List<FileUploadResponse>> GetFiles(string client, string resource, string idresource, string filename = null);
        Task<FileUploadResponse> DownloadFile(string client, string resource, string idresource, string filename);
        Task<bool> ApplyTempFiles(string client, string resource, string idresource, string newidresource);
        Task<bool> DeleteFile(string client, string resource, string idresource, string filename);
    }
} 