using WebApi.FileUploader.Models;

namespace WebApi.FileUploader.Services.Interfaces;

public interface IMongoDbService
{
    Task<List<FileToRemove>> GetFilesToRemoveAsync();
    Task DeleteFileToRemoveAsync(string id);
    Task<FileToRemove> CreateFileToRemoveAsync(string client, string resource, string resourceId);
}