using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Model;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Username { get; set;}
    public string PasswordHash {get; set;}
    public string Role {get; set;}
    public DateTime CreatedAt { get; set;}
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; } = false;
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }
}