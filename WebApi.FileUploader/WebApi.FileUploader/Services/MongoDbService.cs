using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.FileUploader.Infrastructure.Configuration;
using WebApi.FileUploader.Infrastructure.Interfaces;
using WebApi.FileUploader.Models;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Services;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoCollection<FileToRemove> _filesToRemoveCollection;
    private readonly ILogger<MongoDbService> _logger;

    public MongoDbService(
        IOptions<MongoDbOptions> mongoOptions,
        IMongoClientFactory mongoClientFactory,
        ILogger<MongoDbService> logger)
    {
        _logger = logger;

        try
        {
            var client = mongoClientFactory.CreateClient(mongoOptions.Value.ConnectionString);
            var database = client.GetDatabase(mongoOptions.Value.DatabaseName);
            _filesToRemoveCollection = database.GetCollection<FileToRemove>(mongoOptions.Value.FilesToRemoveCollection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB connection");
            throw;
        }
    }

    public async Task<List<FileToRemove>> GetFilesToRemoveAsync()
    {
        try
        {
            return await _filesToRemoveCollection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get files to remove from MongoDB");
            throw;
        }
    }

    public async Task DeleteFileToRemoveAsync(string id)
    {
        try
        {
            await _filesToRemoveCollection.DeleteOneAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file to remove with ID: {Id}", id);
            throw;
        }
    }

    public async Task<FileToRemove> CreateFileToRemoveAsync(string client, string resource, string resourceId)
    {
        try
        {
            var fileToRemove = new FileToRemove
            {
                Client = client,
                Resource = resource,
                ResourceId = resourceId,
                Date = DateTime.UtcNow
            };

            await _filesToRemoveCollection.InsertOneAsync(fileToRemove);
            _logger.LogInformation("Created file removal request: Client={Client}, Resource={Resource}, ResourceId={ResourceId}",
                client, resource, resourceId);

            return fileToRemove;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create file removal request: Client={Client}, Resource={Resource}, ResourceId={ResourceId}",
                client, resource, resourceId);
            throw;
        }
    }
}