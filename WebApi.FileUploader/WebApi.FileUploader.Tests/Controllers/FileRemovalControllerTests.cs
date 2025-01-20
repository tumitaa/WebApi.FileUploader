using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using WebApi.FileUploader.Controllers;
using WebApi.FileUploader.Infrastructure.Dtos;
using WebApi.FileUploader.Models;
using WebApi.FileUploader.Services.Interfaces;
using Xunit;

namespace WebApi.FileUploader.Tests.Controllers;

public class FileRemovalControllerTests
{
    private readonly Mock<IMongoDbService> _mockMongoDbService;
    private readonly Mock<ILogger<FileRemovalController>> _mockLogger;
    private readonly FileRemovalController _controller;

    public FileRemovalControllerTests()
    {
        _mockMongoDbService = new Mock<IMongoDbService>();
        _mockLogger = new Mock<ILogger<FileRemovalController>>();
        _controller = new FileRemovalController(_mockMongoDbService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ScheduleFileRemoval_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new FileRemovalRequest
        {
            Client = "testClient",
            Resource = "testResource",
            ResourceId = "testResourceId"
        };

        var expectedFile = new FileToRemove
        {
            Id = "testId",
            Client = request.Client,
            Resource = request.Resource,
            ResourceId = request.ResourceId,
            Date = DateTime.UtcNow
        };

        _mockMongoDbService
            .Setup(x => x.CreateFileToRemoveAsync(
                request.Client,
                request.Resource,
                request.ResourceId))
            .ReturnsAsync(expectedFile);

        // Act
        var result = await _controller.ScheduleFileRemoval(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseJson = JsonSerializer.Serialize(okResult.Value);
        var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);
        
        Assert.NotNull(response);
        Assert.True(response.ContainsKey("message"));
        Assert.Equal("File removal scheduled successfully", response["message"].GetString());
        Assert.True(response.ContainsKey("id"));
        Assert.Equal(expectedFile.Id, response["id"].GetString());
        Assert.True(response.ContainsKey("scheduledDate"));
    }

    [Fact]
    public async Task ScheduleFileRemoval_WhenServiceThrows_ShouldReturn500()
    {
        // Arrange
        var request = new FileRemovalRequest
        {
            Client = "testClient",
            Resource = "testResource",
            ResourceId = "testResourceId"
        };

        _mockMongoDbService
            .Setup(x => x.CreateFileToRemoveAsync(
                request.Client,
                request.Resource,
                request.ResourceId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.ScheduleFileRemoval(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Error scheduling file removal", statusCodeResult.Value);
    }

    [Theory]
    [InlineData("", "resource", "resourceId")]
    [InlineData("client", "", "resourceId")]
    [InlineData("client", "resource", "")]
    public async Task ScheduleFileRemoval_WithInvalidRequest_ShouldReturnBadRequest(
        string client, string resource, string resourceId)
    {
        // Arrange
        var request = new FileRemovalRequest
        {
            Client = client,
            Resource = resource,
            ResourceId = resourceId
        };

        _controller.ModelState.AddModelError("test", "test error");

        // Act
        var result = await _controller.ScheduleFileRemoval(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
} 