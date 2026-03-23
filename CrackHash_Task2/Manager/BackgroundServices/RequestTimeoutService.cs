using Manager.Services;

namespace Manager.BackgroundServices;

public class RequestTimeoutService(RequestStateService requestStateService) : BackgroundService
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan CleanupTtl = TimeSpan.FromMinutes(30);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            requestStateService.CheckTimeouts(Timeout);
            requestStateService.CleanupFinishedRequests(CleanupTtl);
            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}