using MongoDB.Driver;

namespace WebApi.FileUploader.Infrastructure.Interfaces;

public interface IMongoClientFactory
{
    IMongoClient CreateClient(string connectionString);
} 