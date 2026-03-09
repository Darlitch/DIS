using Contract.Api;

namespace Manager.Models;

public class RequestState(string hash, int maxLength, int partCount)
{
    public Guid RequestId { get; } = Guid.NewGuid();
    public string Hash { get; } = hash;
    public int MaxLength { get; } = maxLength;
    public StatusEnum Status { get; set; } = StatusEnum.IN_PROGRESS;
    public List<string> Answers { get; set; } = new List<string>();
    public int PartCount { get; } = partCount;
    public int CompletedParts { get; set; } = 0;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}