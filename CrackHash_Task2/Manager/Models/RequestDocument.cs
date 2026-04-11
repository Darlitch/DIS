using Contract.Api;
using MongoDB.Bson.Serialization.Attributes;

namespace Manager.Models;

public class RequestDocument
{
    [BsonId]
    public Guid RequestId { get; set; }

    public string Hash { get; set; } = string.Empty;
    public int MaxLength { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.IN_PROGRESS;
    public List<string> Answers { get; set; } = [];
    public int PartCount { get; set; }
    public int CompletedParts { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}