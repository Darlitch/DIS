using System.Collections.Concurrent;
using Contract.Api;
using Manager.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Manager.Services;

public class RequestStateService(IMemoryCache cache)
{
    private readonly ConcurrentDictionary<Guid, RequestState> _requests = new();

    public RequestState CreateRequest(string hash, int maxLentgth, int totalParts)
    {
        var request = new RequestState(hash, maxLentgth, totalParts);
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
        }
        if (request.CompletedParts == request.PartCount)
        {
            request.Status = StatusEnum.READY;
        }
    }

    public void CleanupOldRequests(TimeSpan ttl) // доделать, хуета
    {
        var now = DateTime.UtcNow;
        foreach (var item in _requests)
        {
            if (now - item.Value.CreatedAt > ttl)
            {
                _requests.TryRemove(item.Key, out _);
            }
        }
    }

    public void MarkError(Guid requestId)
    {
        if (_requests.TryGetValue(requestId, out var request))
        {
            request.Status = StatusEnum.ERROR;
        }
    }
    
    public void SaveToCache(string hash, int len, IReadOnlyList<string> answers)
    {
        cache.Set($"{hash}:{len}", answers, TimeSpan.FromMinutes(60));
    }

    public bool TryGetCached(string hash, int len, out IReadOnlyList<string>? answers)
    {
        return cache.TryGetValue($"{hash}:{len}", out answers);
    }
}