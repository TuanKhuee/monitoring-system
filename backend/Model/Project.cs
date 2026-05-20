using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Model;

public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string Name { get; set;}
    public string Description { get; set;}
    public DateTime CreatedAt { get; set;}
    public DateTime UpdatedAt { get; set;}
    public string Status { get; set;}
    public string OwnerId { get; set;}
    public List<string> Members { get; set;}
    public string ProjectCode { get; set;}
    public string ProjectUrl { get; set;}
    public string RepositoryUrl { get; set;}
}