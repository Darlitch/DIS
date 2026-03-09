using Manager.Services;

namespace Manager.BackgroundServices;

public class RequestTimeoutService(RequestStateService requestStateService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
    }
}