using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Model;

public class MonitorLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}
    [BsonRepresentation(BsonType.ObjectId)]
    public string ServiceId {get; set;} = string.Empty;
    public int Status {get; set;}
    public long ResponseTimeMs {get; set;}
    public int? StatusCode {get;set;}
    public string? ErrorMassage {get;set;}
    public DateTime CheckedAt {get;set;} = DateTime.UtcNow;
}