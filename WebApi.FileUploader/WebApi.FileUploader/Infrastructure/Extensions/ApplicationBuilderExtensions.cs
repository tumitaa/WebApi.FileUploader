using Hangfire;
using Microsoft.Extensions.FileProviders;
using WebApi.FileUploader.Infrastructure.Middleware;
using WebApi.FileUploader.Services;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Global error handling
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Development specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            ConfigureDevelopmentFeatures(app);
        }

        ConfigureSecurityMiddleware(app);
        ConfigureStaticFiles(app);
        ConfigureRouting(app);
        ConfigureHangfire(app);

        return app;
    }

    private static void ConfigureSecurityMiddleware(WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseCors("AllowAllOrigins");
        app.UseAuthorization();
    }

    private static void ConfigureStaticFiles(WebApplication app)
    {
        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
            RequestPath = ""
        };
        app.UseStaticFiles(staticFileOptions);

        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.DefaultFileNames.Clear();
        defaultFilesOptions.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(defaultFilesOptions);
    }

    private static void ConfigureDevelopmentFeatures(WebApplication app)
    {
        app.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
            RequestPath = ""
        });
    }

    private static void ConfigureRouting(WebApplication app)
    {
        app.MapControllers();
    }

    private static void ConfigureHangfire(WebApplication app)
    {
        app.UseHangfireDashboard();
        RecurringJob.AddOrUpdate<IFileCleanupService>(
            "cleanup-temp-files",
            service => service.CleanupOldFilesAsync(),
            Cron.Hourly
        );
        RecurringJob.AddOrUpdate<FileRemovalBackgroundService>(
            "process-files-to-remove",
            service => service.ProcessFilesToRemoveAsync(),
            "*/10 * * * *" // Cron expression for every 10 minutes
        );
    }
}