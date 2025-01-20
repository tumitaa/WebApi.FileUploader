using Microsoft.AspNetCore.Mvc;
using WebApi.FileUploader.Infrastructure.Authentication;
using WebApi.FileUploader.Infrastructure.Common;
using WebApi.FileUploader.Infrastructure.Dtos;
using WebApi.FileUploader.Infrastructure.Exceptions;
using WebApi.FileUploader.Services.Interfaces;
using FileNotFoundException = WebApi.FileUploader.Infrastructure.Exceptions.FileNotFoundException;

namespace WebApi.FileUploader.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(AKeyAuthAttribute))]
    public partial class FileController : ApiControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger, IFileUploadService fileUploadService)
        {
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<object>>> UploadFile([FromForm] IFormFile file, [FromForm] string resource, [FromForm] string idresource)
        {
            if (file == null || file.Length == 0)
                throw new FileUploadException("No file uploaded.");

            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                throw new InvalidFileTypeException("Invalid file type. Only JPG, JPEG, PNG and PDF are allowed.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var result = await _fileUploadService.UploadFile(Client, resource, idresource, file.FileName, memoryStream);

            return ApiResponse<object>.SuccessResult(new
            {
                FileName = result.FileName,
                FileSize = file.Length,
                Base64Content = result.Base64
            });
        }

        [HttpPost("apply")]
        public async Task<ActionResult<ApiResponse<object>>> ApplyFiles([FromBody] ApplyFileRequest request)
        {
            await _fileUploadService.ApplyTempFiles(Client, request.Resource, request.IdResource, request.NewIdResource);
            return ApiResponse<object>.SuccessResult(null, "Files applied successfully");
        }

        [HttpGet("files")]
        public async Task<ActionResult<ApiResponse<object>>> GetFiles([FromQuery] string resource, [FromQuery] string idresource, [FromQuery] string? filename = null)
        {
            var result = await _fileUploadService.GetFiles(Client, resource, idresource, filename);
            return ApiResponse<object>.SuccessResult(result);
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string resource, [FromQuery] string idresource, [FromQuery] string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new FileUploadException("Filename is required.");

            var result = await _fileUploadService.DownloadFile(Client, resource, idresource, filename);
            if (result == null)
                throw new FileNotFoundException(filename);

            try
            {
                byte[] fileBytes = Convert.FromBase64String(result.Base64);
                string contentType = GetContentType(filename);
                return File(fileBytes, contentType, filename);
            }
            catch (Exception ex)
            {
                throw new FileProcessingException("Error processing file download", ex);
            }
        }

        [HttpDelete("delete")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteFile([FromBody] DeleteFileRequest request)
        {
            if (string.IsNullOrEmpty(request.Filename))
                throw new FileUploadException("Filename is required.");

            var result = await _fileUploadService.DeleteFile(Client, request.Resource, request.IdResource, request.Filename);
            if (!result)
                throw new FileNotFoundException(request.Filename);

            return ApiResponse<object>.SuccessResult(null, "File deleted successfully");
        }

        private string GetContentType(string filename)
        {
            return Path.GetExtension(filename).ToLower() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
