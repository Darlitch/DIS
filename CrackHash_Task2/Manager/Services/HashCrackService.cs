using Contract.Api;
using Contract.Api.Enums;
using Contract.Xml;
using Manager.Options;
using Manager.Repositories;
using Manager.Utilities;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class HashCrackService(RequestRepository requestRepository, SubtaskRepository subtaskRepository,
    TaskPublisher taskPublisher, IOptions<WorkerOptions> workerOptions, IOptions<RequestOptions> requestOptions)
{
    private readonly string[] _workersUrls = workerOptions.Value.WorkerUrls;

    public async Task<Guid?> StartCrack(HashCrackDto dto, CancellationToken ct = default)
    {
        var request = await requestRepository.GetAsync(dto.Hash, dto.MaxLength, ct);
        if (request is null)
        {
            var count = await requestRepository.CountInProgressAsync(ct);
            if (count >= requestOptions.Value.MaxActiveRequests)
            {
                return null;
            }
            request = await requestRepository.CreateAsync(dto.Hash,dto.MaxLength,_workersUrls.Length, ct);
            for (var i = 0; i < _workersUrls.Length; i++)
            {
                await subtaskRepository.CreateAsync(request, i, ct);
                var workerTask = new WorkerTaskRequest
                {
                    RequestId = request.RequestId.ToString(),
                    PartNumber = i,
                    PartCount = request.PartCount,
                    Hash = request.Hash,
                    MaxLength = request.MaxLength,
                    Alphabet = CrackAlphabet.GetAlphabet()
                };
                try
                {
                    await taskPublisher.PublishAsync(workerTask, ct);
                    await subtaskRepository.UpdateStatusAsync(request.RequestId, i, SubtaskStatus.QUEUED, ct);
                }
                catch
                {
                    // RabbitMQ недоступен, подзадача останется PENDING_DISPATCH
                }
                
            }
        }
        return request.RequestId;
    }

    public async Task<CrackStatusDto> GetRequestStatus(Guid requestId, CancellationToken ct = default)
    {
        var request = await requestRepository.GetAsync(requestId, ct);
        if (request is null)
        {
            return new CrackStatusDto(RequestStatus.ERROR, null);
        }
        return new CrackStatusDto(request.Status, request.Answers.ToArray());
    }

    public async Task ProcessWorkerResult(WorkerTaskResponse response, CancellationToken ct = default)
    {
        var requestId = Guid.Parse(response.RequestId);
        var answers = response.Answers?.Words ?? [];
        var subtask = await subtaskRepository.GetAsync(requestId, response.PartNumber, ct);
        if (subtask == null || subtask.Status == SubtaskStatus.COMPLETED)
        {
            return;
        }
        await subtaskRepository.UpdateAnswersAsync(requestId, response.PartNumber, answers, ct);
        await requestRepository.AddAnswersAsync(requestId, answers, ct);
        long countNotCompleted = await subtaskRepository.CountNotCompletedAsync(requestId, ct);
        if (countNotCompleted == 0)
        {
            await requestRepository.UpdateStatusAsync(requestId, RequestStatus.READY, ct);
        }
    }
}