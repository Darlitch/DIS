using Contract.Api;
using Contract.Xml;
using Manager.Options;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class HashCrackService(RequestRepository repository, IOptions<WorkerOptions> workerOptions, IOptions<RequestOptions> requestOptions)
{
    private readonly string[] _workersUrls = workerOptions.Value.WorkerUrls;

    public async Task<Guid?> StartCrack(HashCrackDto dto, CancellationToken ct = default)
    {
        var request = await repository.GetAsync(dto.Hash, dto.MaxLength, ct);
        if (request is null)
        {
            var count = await repository.CountInProgressAsync(ct);
            if (count >= requestOptions.Value.MaxActiveRequests)
            {
                return null;
            }
            request = await repository.CreateAsync(dto.Hash,dto.MaxLength,_workersUrls.Length, ct);
        }
        return request.RequestId;
    }

    public async Task<CrackStatusDto> GetRequestStatus(Guid requestId, CancellationToken ct = default)
    {
        var request = await repository.GetAsync(requestId, ct);
        if (request is null)
        {
            return new CrackStatusDto(StatusEnum.ERROR, null);
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