using Contract.Api;
using Contract.Xml;
using Manager.Options;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class HashCrackService(RequestStateService requestStateService, RequestQueueService queue,
    IOptions<WorkerOptions> options)
{
    private readonly string[] _workersUrls = options.Value.WorkerUrls;

    public Guid? StartCrack(HashCrackDto dto)
    {
        if (requestStateService.TryGetCached(dto.Hash, dto.MaxLength, out var cached))
        {
            var cachedRequest = requestStateService.CreateRequest(dto.Hash, dto.MaxLength, 0);
            cachedRequest.Status = StatusEnum.READY;
            cachedRequest.Answers = cached!.ToList();
            cachedRequest.FinishedAt = DateTime.UtcNow;
            cachedRequest.Completion.TrySetResult(true);
            return cachedRequest.RequestId;
        }
        var request = requestStateService.CreateRequest(dto.Hash,dto.MaxLength,_workersUrls.Length);
        if (!queue.Enqueue(request.RequestId))
        {
            request.Status = StatusEnum.ERROR;
            request.FinishedAt = DateTime.UtcNow;
            request.Completion.TrySetResult(false);
            return null;
        }
        return request.RequestId;
    }

    public CrackStatusDto GetRequestStatus(Guid requestId)
    {
        if (!requestStateService.GetRequest(requestId, out var requestState))
        {
            return new CrackStatusDto(StatusEnum.ERROR, null);
        }
        return new CrackStatusDto(requestState!.Status, requestState.Answers.ToArray());
    }

    public void ProcessWorkerResult(WorkerTaskResponse response)
    {
        var requestId = Guid.Parse(response.RequestId);
        var answers = response.Answers?.Words ?? [];
        requestStateService.AddAnswers(requestId, answers);
    }
}