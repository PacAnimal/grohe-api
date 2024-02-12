using Application.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application;

public class NotificationPollerService(IApiClientLockQueue apiClientLockQueue, ILogger<NotificationPollerService> log) : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private string _latestNotificationId;
    private Task _executingTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = ExecuteAsync(_cancellationTokenSource.Token);
        return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var apiClientLock = await apiClientLockQueue.GetLock();
                var apiClient = apiClientLock.ApiClient;
                var latestNotification = await apiClient.GetLatestNotificationId();
                if (latestNotification == _latestNotificationId) continue;
                _latestNotificationId = latestNotification;
                apiClient.FlushCache(); // clear the cache, so that the next request for notifications will get the new one(s)
                log.LogInformation("Latest notification id: {NotificationId}", _latestNotificationId ?? "none");
            }
            finally
            {
                await Task.Delay(Constants.NotificationPollInterval, cancellationToken);                
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        GC.SuppressFinalize(this);
    }
}