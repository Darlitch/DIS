using Contract.Api;
using Contract.Api.Enums;
using Contract.Xml;
using Manager.Options;
using Manager.Repositories;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class HashCrackService(RequestRepository requestRepository, SubtaskRepository subtaskRepository, IOptions<WorkerOptions> workerOptions, IOptions<RequestOptions> requestOptions)
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

    public void ProcessWorkerResult(WorkerTaskResponse response, CancellationToken ct = default)
    {
        var requestId = Guid.Parse(response.RequestId);
        var answers = response.Answers?.Words ?? [];
        // requestStateService.AddAnswers(requestId, answers);
    }
}