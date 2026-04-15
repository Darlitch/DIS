using Contract.Api.Enums;
using Contract.Xml;
using Manager.Repositories;
using Manager.Services;
using Manager.Utilities;

namespace Manager.BackgroundServices;

public class PendingDispatchService(SubtaskRepository subtaskRepository, TaskPublisher taskPublisher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var pendingSubtasks = await subtaskRepository.GetPendingDispatchAsync(ct);
            foreach (var subtask in pendingSubtasks)
            {
                try
                {
                    var workerTask = new WorkerTaskRequest
                    {
                        RequestId = subtask.RequestId.ToString(),
                        PartNumber = subtask.PartNumber,
                        PartCount = subtask.PartCount,
                        Hash = subtask.Hash,
                        MaxLength = subtask.MaxLength,
                        Alphabet = CrackAlphabet.GetAlphabet()
                    };

                    await taskPublisher.PublishAsync(workerTask, ct);
                    await subtaskRepository.UpdateStatusAsync(subtask.RequestId, subtask.PartNumber, SubtaskStatus.QUEUED, ct);
                }
                catch
                {
                    // RabbitMQ недоступен, подзадача останется PENDING_DISPATCH
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}