using System.Collections.Concurrent;

namespace Manager.Services;

public class RequestQueueService
{
    private readonly ConcurrentQueue<Guid> _requestQueue = new();
    private const int MaxQueueSize = 100;

    public bool Enqueue(Guid requestId)
    {
        if (_requestQueue.Count >= MaxQueueSize)
        {
            return false;
        }
        _requestQueue.Enqueue(requestId);
        return true;
    }

    public bool TryDequeue(out Guid requestId)
    {
        return _requestQueue.TryDequeue(out requestId);
    }
}