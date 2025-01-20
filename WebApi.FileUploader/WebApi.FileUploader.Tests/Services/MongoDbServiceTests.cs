using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using WebApi.FileUploader.Infrastructure.Configuration;
using WebApi.FileUploader.Infrastructure.Interfaces;
using WebApi.FileUploader.Models;
using WebApi.FileUploader.Services;
using Xunit;

namespace WebApi.FileUploader.Tests.Services;

public class MongoDbServiceTests
{
    private readonly Mock<IMongoCollection<FileToRemove>> _mockCollection;
    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IOptions<MongoDbOptions>> _mockOptions;
    private readonly Mock<ILogger<MongoDbService>> _mockLogger;
    private readonly MongoDbService _service;
    private readonly MongoDbOptions _options;

    public MongoDbServiceTests()
    {
        _mockCollection = new Mock<IMongoCollection<FileToRemove>>();
        _mockDb = new Mock<IMongoDatabase>();
        _mockClient = new Mock<IMongoClient>();
        _mockOptions = new Mock<IOptions<MongoDbOptions>>();
        _mockLogger = new Mock<ILogger<MongoDbService>>();

        _options = new MongoDbOptions
        {
            ConnectionString = "mongodb://test",
            DatabaseName = "testdb",
            FilesToRemoveCollection = "testcollection"
        };

        _mockOptions
            .Setup(x => x.Value)
            .Returns(_options);

        _mockDb
            .Setup(d => d.GetCollection<FileToRemove>(_options.FilesToRemoveCollection, null))
            .Returns(_mockCollection.Object);

        _mockClient
            .Setup(c => c.GetDatabase(_options.DatabaseName, null))
            .Returns(_mockDb.Object);

        // Mock the MongoDB client factory
        var mockClientFactory = new Mock<IMongoClientFactory>();
        mockClientFactory
            .Setup(f => f.CreateClient(_options.ConnectionString))
            .Returns(_mockClient.Object);

        _service = new MongoDbService(_mockOptions.Object, mockClientFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetFilesToRemoveAsync_ShouldReturnList()
    {
        // Arrange
        var expectedFiles = new List<FileToRemove>
        {
            new() { Id = "1", Client = "test", Resource = "res", ResourceId = "123" },
            new() { Id = "2", Client = "test", Resource = "res", ResourceId = "456" }
        };

        var mockCursor = new Mock<IAsyncCursor<FileToRemove>>();
        mockCursor
            .Setup(c => c.Current)
            .Returns(expectedFiles);
        mockCursor
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<FileToRemove>>(),
                It.IsAny<FindOptions<FileToRemove>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetFilesToRemoveAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedFiles.Count, result.Count);
        Assert.Equal(expectedFiles[0].Id, result[0].Id);
        Assert.Equal(expectedFiles[1].Id, result[1].Id);
        _mockCollection.Verify(
            c => c.FindAsync(
                It.IsAny<FilterDefinition<FileToRemove>>(),
                It.IsAny<FindOptions<FileToRemove>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteFileToRemoveAsync_ShouldCallDeleteOne()
    {
        // Arrange
        var id = "testId";
        var deleteResult = new DeleteResult.Acknowledged(1);

        _mockCollection
            .Setup(c => c.DeleteOneAsync(
                It.IsAny<ExpressionFilterDefinition<FileToRemove>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act
        await _service.DeleteFileToRemoveAsync(id);

        // Assert
        _mockCollection.Verify(
            c => c.DeleteOneAsync(
                It.IsAny<ExpressionFilterDefinition<FileToRemove>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateFileToRemoveAsync_ShouldInsertAndReturnDocument()
    {
        // Arrange
        var client = "testClient";
        var resource = "testResource";
        var resourceId = "testResourceId";

        _mockCollection
            .Setup(c => c.InsertOneAsync(
                It.IsAny<FileToRemove>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateFileToRemoveAsync(client, resource, resourceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(client, result.Client);
        Assert.Equal(resource, result.Resource);
        Assert.Equal(resourceId, result.ResourceId);
        Assert.NotEqual(default, result.Date);

        _mockCollection.Verify(
            c => c.InsertOneAsync(
                It.Is<FileToRemove>(f =>
                    f.Client == client &&
                    f.Resource == resource &&
                    f.ResourceId == resourceId),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
} 