using Contract.Api;
using Contract.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Manager.Models;

public class RequestDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid RequestId { get; set; }

    public string Hash { get; set; } = string.Empty;
    public int MaxLength { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.IN_PROGRESS;
    public List<string> Answers { get; set; } = [];
    public int PartCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}