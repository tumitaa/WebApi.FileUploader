using System.Net;
using System.Text.Json;
using WebApi.FileUploader.Infrastructure.Common;
using WebApi.FileUploader.Infrastructure.Exceptions;

namespace WebApi.FileUploader.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            FileUploadException fileEx => (fileEx.StatusCode, fileEx.Message),
            ArgumentException _ => (StatusCodes.Status400BadRequest, "Invalid arguments provided"),
            UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "Unauthorized access"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        response.StatusCode = statusCode;

        var result = ApiResponse<object>.ErrorResult(message);

        // Log the exception
        var logLevel = statusCode == StatusCodes.Status500InternalServerError 
            ? LogLevel.Error 
            : LogLevel.Warning;

        _logger.Log(logLevel, exception, 
            "An error occurred processing request {Path}: {Message}", 
            context.Request.Path, 
            exception.Message);

        // Include stack trace only in development
        if (_environment.IsDevelopment())
        {
            result.Errors.Add(exception.StackTrace);
        }

        await response.WriteAsync(JsonSerializer.Serialize(result));
    }
} 