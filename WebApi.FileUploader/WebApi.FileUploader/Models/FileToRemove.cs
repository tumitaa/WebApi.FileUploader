using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApi.FileUploader.Models;

public class FileToRemove
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string Client { get; set; }
    
    public string Resource { get; set; }
    
    public string ResourceId { get; set; }
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Date { get; set; }
} 