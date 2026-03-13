using System.Collections.Concurrent;
using Contract.Api;
using Manager.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Manager.Services;

public class RequestStateService(IMemoryCache cache)
{
    private readonly ConcurrentDictionary<Guid, RequestState> _requests = new();

    public RequestState CreateRequest(string hash, int maxLength, int totalParts)
    {
        var request = new RequestState(hash, maxLength, totalParts);
        _requests[request.RequestId] = request;
        return request;
    }

    public bool GetRequest(Guid requestId, out RequestState? request)
    {
        return _requests.TryGetValue(requestId, out request);
    }

    public void AddAnswers(Guid requestId, List<string> answers)
    {
        if (!_requests.TryGetValue(requestId, out var request))
        {
            return;
        }
        lock (request)
        {
            request.Answers.AddRange(answers);
            request.CompletedParts++;
            if (request.CompletedParts == request.PartCount)
            {
                request.Status = StatusEnum.READY;
                request.FinishedAt = DateTime.UtcNow;
                SaveToCache(request.Hash, request.MaxLength, request.Answers);
                request.Completion.TrySetResult(true);
            }
        }
    }

    public void CleanupFinishedRequests(TimeSpan ttl)
    {
        var now = DateTime.UtcNow;
        foreach (var (id, request) in _requests)
        {
            if (request.FinishedAt == null)
                continue;
            if (now - request.FinishedAt > ttl)
            {
                _requests.TryRemove(id, out _);
            }
        }
    }
    
    public void CheckTimeouts(TimeSpan timeout)
    {
        var now = DateTime.UtcNow;
        foreach (var request in _requests.Values)
        {
            if (request.Status != StatusEnum.IN_PROGRESS)
                continue;
            if (request.StartedAt == null)
                continue;
            if (now - request.StartedAt > timeout)
            {
                request.Status = request.Answers.Count > 0 ? StatusEnum.PARTIAL_RESULT : StatusEnum.ERROR;
                request.FinishedAt = DateTime.UtcNow;
                request.Completion.TrySetResult(false);
            }
        }
    }
    
    private void SaveToCache(string hash, int len, IReadOnlyList<string> answers)
    {
        cache.Set($"{hash}:{len}", answers, TimeSpan.FromMinutes(60));
    }

    public bool TryGetCached(string hash, int len, out IReadOnlyList<string>? answers)
    {
        return cache.TryGetValue($"{hash}:{len}", out answers);
    }
}