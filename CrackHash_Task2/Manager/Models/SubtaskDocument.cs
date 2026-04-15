using Contract.Api;
using Contract.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Manager.Models;

public class SubtaskDocument
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid RequestId { get; set; }
    public string Hash { get; set; } = string.Empty;
    public int MaxLength { get; set; }
    public SubtaskStatus Status { get; set; } = SubtaskStatus.PENDING_DISPATCH;
    public List<string> Answers { get; set; } = [];
    public int PartNumber { get; set; }
    public int PartCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}