using Contract.Api;
using Contract.Api.Enums;

namespace Manager.Models;

public class RequestState(string hash, int maxLength, int partCount)
{
    public Guid RequestId { get; } = Guid.NewGuid();
    public string Hash { get; } = hash;
    public int MaxLength { get; } = maxLength;
    public RequestStatus RequestStatus { get; set; } = RequestStatus.IN_PROGRESS;
    public List<string> Answers { get; set; } = new List<string>();
    public int PartCount { get; } = partCount;
    public int CompletedParts { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TaskCompletionSource<bool> Completion { get; } = new();
}