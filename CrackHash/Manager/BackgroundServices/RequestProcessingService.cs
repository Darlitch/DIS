using Contract.Xml;
using Manager.Clients;
using Manager.Models;
using Manager.Options;
using Manager.Services;
using Manager.Utilities;
using Microsoft.Extensions.Options;

namespace Manager.BackgroundServices;

public class RequestProcessingService(RequestStateService requestStateService, RequestQueueService queue,
    WorkerClient workerClient, IOptions<WorkerOptions> options) : BackgroundService
{
    private readonly string[] _workerUrls = options.Value.WorkerUrls;
    private readonly Alphabet _alphabet = CrackAlphabet.GetAlphabet();
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!queue.TryDequeue(out var requestId))
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }
            if(!requestStateService.GetRequest(requestId, out var request))
            {
                continue;
            }
            request!.StartedAt = DateTime.UtcNow;
            await ProcessRequest(request);
            await request.Completion.Task;
        }
    }

    private async Task ProcessRequest(RequestState request)
    {
        for (var i = 0; i < _workerUrls.Length; i++)
        {
            var task = new WorkerTaskRequest
            {
                RequestId = request.RequestId.ToString(),
                PartNumber = i,
                PartCount = _workerUrls.Length,
                Hash = request.Hash,
                MaxLength = request.MaxLength,
                Alphabet = _alphabet
            };
            await workerClient.SendTaskAsync(_workerUrls[i], task);
        }
    }
}