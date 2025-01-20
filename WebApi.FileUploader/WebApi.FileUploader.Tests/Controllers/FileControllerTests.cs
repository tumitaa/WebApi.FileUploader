using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using WebApi.FileUploader.Infrastructure.Common;
using WebApi.FileUploader.Infrastructure.Dtos;
using WebApi.FileUploader.Infrastructure.Exceptions;
using WebApi.FileUploader.Controllers;
using WebApi.FileUploader.Services.Interfaces;
using FileNotFoundException = WebApi.FileUploader.Infrastructure.Exceptions.FileNotFoundException;
using FileUploadResponse = WebApi.FileUploader.Services.FileUploadResponse;
using Xunit;

namespace WebApi.FileUploader.Tests.Controllers
{
    public class FileControllerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService;
        private readonly Mock<ILogger<FileController>> _mockLogger;
        private readonly FileController _controller;

        public FileControllerTests()
        {
            _mockFileUploadService = new Mock<IFileUploadService>();
            _mockLogger = new Mock<ILogger<FileController>>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Client-Hash"] = "test-hash";

            _controller = new FileController(_mockLogger.Object, _mockFileUploadService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public async Task UploadFile_WithValidFile_ShouldReturnSuccess()
        {
            // Arrange
            var content = "test content";
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var file = new FormFile(new MemoryStream(contentBytes), 0, contentBytes.Length, "Data", "test.jpg");
            var resource = "test-resource";
            var idResource = "test-id";

            _mockFileUploadService.Setup(x => x.UploadFile(It.IsAny<string>(), resource, idResource, file.FileName, It.IsAny<MemoryStream>()))
                .ReturnsAsync(new FileUploadResponse("test.jpg", Convert.ToBase64String(contentBytes), contentBytes.Length));

            // Act
            var result = await _controller.UploadFile(file, resource, idResource);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<object>>>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
        }

        [Fact]
        public async Task UploadFile_WithNullFile_ShouldThrowException()
        {
            // Arrange
            IFormFile file = null;
            var resource = "test-resource";
            var idResource = "test-id";

            // Act & Assert
            await Assert.ThrowsAsync<FileUploadException>(() => _controller.UploadFile(file, resource, idResource));
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test.exe")]
        [InlineData("test.txt")]
        public async Task UploadFile_WithInvalidFileType_ShouldThrowException(string fileName)
        {
            // Arrange
            var content = "test content";
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var file = new FormFile(new MemoryStream(contentBytes), 0, contentBytes.Length, "Data", fileName);
            var resource = "test-resource";
            var idResource = "test-id";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidFileTypeException>(() => _controller.UploadFile(file, resource, idResource));
        }

        [Fact]
        public async Task ApplyFiles_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var request = new ApplyFileRequest
            {
                Resource = "test-resource",
                IdResource = "test-id",
                NewIdResource = "new-test-id"
            };

            _mockFileUploadService.Setup(x => x.ApplyTempFiles(It.IsAny<string>(), request.Resource, request.IdResource, request.NewIdResource))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ApplyFiles(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<object>>>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("Files applied successfully", apiResponse.Message);
        }

        [Fact]
        public async Task GetFiles_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var resource = "test-resource";
            var idResource = "test-id";
            var fileName = "test.jpg";

            var content1 = "test content 1";
            var content2 = "test content 2";
            var contentBytes1 = Encoding.UTF8.GetBytes(content1);
            var contentBytes2 = Encoding.UTF8.GetBytes(content2);

            var expectedFiles = new List<FileUploadResponse>
            {
                new FileUploadResponse("test1.jpg", Convert.ToBase64String(contentBytes1), contentBytes1.Length),
                new FileUploadResponse("test2.jpg", Convert.ToBase64String(contentBytes2), contentBytes2.Length)
            };

            _mockFileUploadService.Setup(x => x.GetFiles(It.IsAny<string>(), resource, idResource, fileName))
                .ReturnsAsync(expectedFiles);

            // Act
            var result = await _controller.GetFiles(resource, idResource, fileName);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<object>>>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
        }

        [Fact]
        public async Task DownloadFile_WithValidRequest_ShouldReturnFile()
        {
            // Arrange
            var resource = "test-resource";
            var idResource = "test-id";
            var fileName = "test.jpg";
            var content = "test content";
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var base64Content = Convert.ToBase64String(contentBytes);

            _mockFileUploadService.Setup(x => x.DownloadFile(It.IsAny<string>(), resource, idResource, fileName))
                .ReturnsAsync(new FileUploadResponse(fileName, base64Content, contentBytes.Length));

            // Act
            var result = await _controller.DownloadFile(resource, idResource, fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/jpeg", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);
        }

        [Fact]
        public async Task DownloadFile_WithEmptyFileName_ShouldThrowException()
        {
            // Arrange
            var resource = "test-resource";
            var idResource = "test-id";
            var fileName = "";

            // Act & Assert
            await Assert.ThrowsAsync<FileUploadException>(() => _controller.DownloadFile(resource, idResource, fileName));
        }

        [Fact]
        public async Task DownloadFile_WhenFileNotFound_ShouldThrowException()
        {
            // Arrange
            var resource = "test-resource";
            var idResource = "test-id";
            var fileName = "test.jpg";

            _mockFileUploadService.Setup(x => x.DownloadFile(It.IsAny<string>(), resource, idResource, fileName))
                .ThrowsAsync(new FileNotFoundException("File not found"));

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _controller.DownloadFile(resource, idResource, fileName));
        }

        [Fact]
        public async Task DeleteFile_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var request = new DeleteFileRequest
            {
                Resource = "test-resource",
                IdResource = "test-id",
                Filename = "test.jpg"
            };

            _mockFileUploadService.Setup(x => x.DeleteFile(It.IsAny<string>(), request.Resource, request.IdResource, request.Filename))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteFile(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<object>>>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("File deleted successfully", apiResponse.Message);
        }

        [Fact]
        public async Task DeleteFile_WithEmptyFileName_ShouldThrowException()
        {
            // Arrange
            var request = new DeleteFileRequest
            {
                Resource = "test-resource",
                IdResource = "test-id",
                Filename = ""
            };

            // Act & Assert
            await Assert.ThrowsAsync<FileUploadException>(() => _controller.DeleteFile(request));
        }

        [Fact]
        public async Task DeleteFile_WhenFileNotFound_ShouldThrowException()
        {
            // Arrange
            var request = new DeleteFileRequest
            {
                Resource = "test-resource",
                IdResource = "test-id",
                Filename = "test.jpg"
            };

            _mockFileUploadService.Setup(x => x.DeleteFile(It.IsAny<string>(), request.Resource, request.IdResource, request.Filename))
                .ThrowsAsync(new FileNotFoundException("File not found"));

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _controller.DeleteFile(request));
        }
    }
} 