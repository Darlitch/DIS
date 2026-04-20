using Manager.Repositories;

namespace Manager.BackgroundServices;

public class SubtaskRecoveryService(SubtaskRepository subtaskRepository) : BackgroundService
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await subtaskRepository.ResetStaleQueuedToPendingAsync(Timeout, ct);
            await Task.Delay(Interval, ct);
        }
    }
}