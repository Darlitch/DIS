using Contract.Api;
using Contract.Xml;
using Manager.Clients;
using Manager.Options;
using Manager.Utilities;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class HashCrackService(RequestStateService requestStateService, WorkerClient workerClient,
    IOptions<WorkerOptions> options)
{
    private readonly string[] _workersUrls = options.Value.WorkerUrls;

    private readonly Alphabet _alphabet = CrackAlphabet.GetAlphabet();

    public async Task<Guid> StartCrackAsync(HashCrackDto dto)
    {
        var request = requestStateService.CreateRequest(_workersUrls.Length);
        for (var i = 0; i < _workersUrls.Length; i++)
        {
            var task = new WorkerTaskRequest
            {
                RequestId = request.RequestId.ToString(),
                PartNumber = i,
                PartCount = _workersUrls.Length,
                Hash = dto.hash,
                MaxLength = dto.maxLength,
                Alphabet = _alphabet
            };
            await workerClient.SendTaskAsync(_workersUrls[i], task);
        }
        return request.RequestId;
    }

    public CrackStatusDto GetRequestStatusAsync(Guid requestId)
    {
        if (!requestStateService.GetRequest(requestId, out var requestState))
        {
            return new CrackStatusDto(StatusEnum.ERROR, null);
        }
        return new CrackStatusDto(requestState.Status, requestState.Answers.ToArray());
    }
}