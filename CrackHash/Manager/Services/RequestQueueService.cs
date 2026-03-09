using System.Collections.Concurrent;

namespace Manager.Services;

public class RequestQueueService
{
    private readonly ConcurrentQueue<Guid> _requestQueue = new();

    public void Enqueue(Guid requestId)
    {
        _requestQueue.Enqueue(requestId);
    }

    public bool TryDequeue(out Guid requestId)
    {
        return _requestQueue.TryDequeue(out requestId);
    }
}