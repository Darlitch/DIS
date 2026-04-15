using Contract.Api;
using Contract.Api.Enums;
using Contract.Xml;
using Manager.Options;
using Manager.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Manager.Services;

public class HashCrackService(RequestRepository requestRepository, SubtaskRepository subtaskRepository,
    IOptions<WorkerOptions> workerOptions,IMongoClient mongoClient, IOptions<RequestOptions> requestOptions)
{
    private readonly string[] _workersUrls = workerOptions.Value.WorkerUrls;

    public async Task<Guid?> StartCrack(HashCrackDto dto, CancellationToken ct = default)
    {
        var request = await requestRepository.GetAsync(dto.Hash, dto.MaxLength, ct);
        if (request is not null)
        {
            return request.RequestId;
        }
        var count = await requestRepository.CountInProgressAsync(ct);
        if (count >= requestOptions.Value.MaxActiveRequests)
        {
            return null;
        }

        for (var attempt = 1; attempt <= requestOptions.Value.MaxAttempts; attempt++)
        {
            using var session = await mongoClient.StartSessionAsync(cancellationToken: ct);
            session.StartTransaction();
            try
            {
                request = await requestRepository.CreateAsync(session, dto.Hash, dto.MaxLength, _workersUrls.Length,
                    ct);
                await subtaskRepository.CreateManyAsync(session, request, ct);

                await session.CommitTransactionAsync(ct);
                return request.RequestId;
            }
            catch (MongoException ex) when (
                attempt < requestOptions.Value.MaxAttempts &&
                (ex.HasErrorLabel("TransientTransactionError") ||
                 ex.HasErrorLabel("UnknownTransactionCommitResult")))
            {
                await TryAbortAsync(session, ct);
                await Task.Delay(TimeSpan.FromMilliseconds(1000), ct);
            }
            catch
            {
                await TryAbortAsync(session, ct);
                throw;
            }
        }
        throw new InvalidOperationException("Failed to create request after retries.");
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
    
    private static async Task TryAbortAsync(IClientSessionHandle session, CancellationToken ct)
    {
        try
        {
            await session.AbortTransactionAsync(ct);
        }
        catch
        {
            // 
        }
    }
}