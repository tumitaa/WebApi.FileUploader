using WebApi.FileUploader.Services;
using WebApi.FileUploader.Services.Interfaces;
using WebApi.FileUploader.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
var app = builder.Build();
app.ConfigurePipeline();
app.Run();
