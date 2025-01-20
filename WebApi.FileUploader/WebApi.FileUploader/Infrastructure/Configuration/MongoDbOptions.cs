namespace WebApi.FileUploader.Infrastructure.Configuration;

public class MongoDbOptions
{
    public const string SectionName = "MongoDB";
    
    public string ConnectionString { get; set; } = "mongodb://cheetor.local:27017";
    public string DatabaseName { get; set; } = "filemanager";
    public string FilesToRemoveCollection { get; set; } = "filesToRemove";
} 