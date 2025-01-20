using Amazon.S3;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebApi.FileUploader.Infrastructure.Authentication;
using WebApi.FileUploader.Infrastructure.Configuration;
using WebApi.FileUploader.Infrastructure.Interfaces;
using WebApi.FileUploader.Infrastructure.Services;
using WebApi.FileUploader.Services;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.ConfigureOptions()
               .ConfigureAWS()
               .ConfigureApplicationServices()
               .ConfigureHangfire()
               .ConfigureCors()
               .ConfigureRedis()
               .ConfigureMongoDB();

        return builder;
    }

    private static WebApplicationBuilder ConfigureOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<S3Options>(builder.Configuration.GetSection(S3Options.SectionName));
        builder.Services.Configure<FileCleanupOptions>(builder.Configuration.GetSection(FileCleanupOptions.SectionName));
        builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(MongoDbOptions.SectionName));
        return builder;
    }

    private static WebApplicationBuilder ConfigureAWS(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3Options = sp.GetRequiredService<IOptions<S3Options>>().Value;
            return new AmazonS3Client(
                s3Options.AccessKey,
                s3Options.SecretKey,
                new AmazonS3Config
                {
                    ServiceURL = s3Options.BaseUrl,
                    ForcePathStyle = true
                });
        });
        return builder;
    }

    private static WebApplicationBuilder ConfigureApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDriveService, DriveService>()
                       .AddScoped<IFileStorageService, FileStorageService>()
                       .AddScoped<IImageProcessingService, ImageProcessingService>()
                       .AddScoped<FileUploadService>()
                       .AddScoped<AKeyAuthAttribute>()
                       .AddTransient<IFileCleanupService, FileCleanupService>()
                       .AddScoped<IMongoDbService, MongoDbService>()
                       .AddScoped<FileRemovalBackgroundService>()
                       .AddHttpClient();
        return builder;
    }

    private static WebApplicationBuilder ConfigureHangfire(this WebApplicationBuilder builder)
    {
        builder.Services.AddHangfire(config =>
        {
            config.UseSimpleAssemblyNameTypeSerializer()
                  .UseDefaultTypeSerializer()
                  .UseMemoryStorage();
        });
        builder.Services.AddHangfireServer();
        return builder;
    }

    private static WebApplicationBuilder ConfigureCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
        return builder;
    }

    private static WebApplicationBuilder ConfigureRedis(this WebApplicationBuilder builder)
    {
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        return builder;
    }

    private static WebApplicationBuilder ConfigureMongoDB(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMongoClientFactory, MongoClientFactory>();
        return builder;
    }
}