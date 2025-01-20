using MongoDB.Driver;
using WebApi.FileUploader.Infrastructure.Interfaces;

namespace WebApi.FileUploader.Infrastructure.Services;

public class MongoClientFactory : IMongoClientFactory
{
    public IMongoClient CreateClient(string connectionString)
    {
        return new MongoClient(connectionString);
    }
} 