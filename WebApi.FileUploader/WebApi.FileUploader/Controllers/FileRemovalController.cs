using Microsoft.AspNetCore.Mvc;
using WebApi.FileUploader.Infrastructure.Authentication;
using WebApi.FileUploader.Infrastructure.Dtos;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(AKeyAuthAttribute))]
public class FileRemovalController : ControllerBase
{
    private readonly IMongoDbService _mongoDbService;
    private readonly ILogger<FileRemovalController> _logger;

    public FileRemovalController(
        IMongoDbService mongoDbService,
        ILogger<FileRemovalController> logger)
    {
        _mongoDbService = mongoDbService;
        _logger = logger;
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleFileRemoval([FromBody] FileRemovalRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var fileToRemove = await _mongoDbService.CreateFileToRemoveAsync(
                request.Client,
                request.Resource,
                request.ResourceId);

            return Ok(new
            {
                message = "File removal scheduled successfully",
                id = fileToRemove.Id,
                scheduledDate = fileToRemove.Date
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling file removal for Client={Client}, Resource={Resource}, ResourceId={ResourceId}",
                request.Client, request.Resource, request.ResourceId);
            return StatusCode(500, "Error scheduling file removal");
        }
    }
}