using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using WebApi.FileUploader.Models;
using WebApi.FileUploader.Services;
using WebApi.FileUploader.Services.Interfaces;
using Xunit;

namespace WebApi.FileUploader.Tests.Services;

public class FileRemovalBackgroundServiceTests
{
    private readonly Mock<IMongoDbService> _mockMongoDbService;
    private readonly Mock<IDriveService> _mockDriveService;
    private readonly Mock<ILogger<FileRemovalBackgroundService>> _mockLogger;
    private readonly FileRemovalBackgroundService _service;

    public FileRemovalBackgroundServiceTests()
    {
        _mockMongoDbService = new Mock<IMongoDbService>();
        _mockDriveService = new Mock<IDriveService>();
        _mockLogger = new Mock<ILogger<FileRemovalBackgroundService>>();

        _service = new FileRemovalBackgroundService(
            _mockMongoDbService.Object,
            _mockDriveService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessFilesToRemoveAsync_WithNoFiles_ShouldComplete()
    {
        // Arrange
        _mockMongoDbService
            .Setup(x => x.GetFilesToRemoveAsync())
            .ReturnsAsync(new List<FileToRemove>());

        // Act
        await _service.ProcessFilesToRemoveAsync();

        // Assert
        _mockDriveService.Verify(
            x => x.DeleteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _mockMongoDbService.Verify(
            x => x.DeleteFileToRemoveAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessFilesToRemoveAsync_WithFiles_ShouldProcessAll()
    {
        // Arrange
        var files = new List<FileToRemove>
        {
            new() { Id = "1", Client = "test1", Resource = "res1", ResourceId = "123" },
            new() { Id = "2", Client = "test2", Resource = "res2", ResourceId = "456" }
        };

        _mockMongoDbService
            .Setup(x => x.GetFilesToRemoveAsync())
            .ReturnsAsync(files);

        _mockDriveService
            .Setup(x => x.DeleteFile(
                It.Is<string>(s => s == "test1/res1" || s == "test2/res2"),
                It.Is<string>(s => s == "123" || s == "456"),
                It.Is<string>(s => s == string.Empty)))
            .ReturnsAsync(true);

        // Act
        await _service.ProcessFilesToRemoveAsync();

        // Assert
        _mockDriveService.Verify(
            x => x.DeleteFile(
                It.Is<string>(s => s == "test1/res1" || s == "test2/res2"),
                It.Is<string>(s => s == "123" || s == "456"),
                It.Is<string>(s => s == string.Empty)),
            Times.Exactly(2));
        _mockMongoDbService.Verify(
            x => x.DeleteFileToRemoveAsync(It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessFilesToRemoveAsync_WhenDeleteFails_ShouldNotRemoveFromMongo()
    {
        // Arrange
        var files = new List<FileToRemove>
        {
            new() { Id = "1", Client = "test", Resource = "res", ResourceId = "123" }
        };

        _mockMongoDbService
            .Setup(x => x.GetFilesToRemoveAsync())
            .ReturnsAsync(files);

        _mockDriveService
            .Setup(x => x.DeleteFile("test/res", "123", string.Empty))
            .ReturnsAsync(false);

        // Act
        await _service.ProcessFilesToRemoveAsync();

        // Assert
        _mockDriveService.Verify(
            x => x.DeleteFile("test/res", "123", string.Empty),
            Times.Once);
        _mockMongoDbService.Verify(
            x => x.DeleteFileToRemoveAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessFilesToRemoveAsync_WhenExceptionOccurs_ShouldContinueProcessing()
    {
        // Arrange
        var files = new List<FileToRemove>
        {
            new() { Id = "1", Client = "test1", Resource = "res1", ResourceId = "123" },
            new() { Id = "2", Client = "test2", Resource = "res2", ResourceId = "456" }
        };

        _mockMongoDbService
            .Setup(x => x.GetFilesToRemoveAsync())
            .ReturnsAsync(files);

        _mockDriveService
            .Setup(x => x.DeleteFile("test1/res1", "123", string.Empty))
            .ThrowsAsync(new Exception("Test exception"));

        _mockDriveService
            .Setup(x => x.DeleteFile("test2/res2", "456", string.Empty))
            .ReturnsAsync(true);

        // Act
        await _service.ProcessFilesToRemoveAsync();

        // Assert
        _mockDriveService.Verify(
            x => x.DeleteFile("test1/res1", "123", string.Empty),
            Times.Once);
        _mockDriveService.Verify(
            x => x.DeleteFile("test2/res2", "456", string.Empty),
            Times.Once);
        _mockMongoDbService.Verify(
            x => x.DeleteFileToRemoveAsync(It.IsAny<string>()),
            Times.Once);
    }
} 