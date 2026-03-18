using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Document.Service.Models;

public class Document
{
    [BsonId] // MongoDB'deki _id
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    //document bilgileri
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
