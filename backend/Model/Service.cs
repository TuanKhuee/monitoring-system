using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Model;

public class Service
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty; 
    public int Port { get; set; } 
    public string Protocol { get; set; } = "Http"; 
    public string? HealthEndpoint { get; set; } 
    public int IntervalSeconds { get; set; } = 60; 
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}