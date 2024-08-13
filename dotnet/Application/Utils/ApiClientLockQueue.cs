using Cathedral.Utils;

namespace Application.Utils;

public interface IApiClientLockQueue
{
    Task<Lock> GetLock();
    public class Lock(IApiClient apiClient, Func<Task> onDispose) : IAsyncDisposable
    {
        private int _disposed;
        private readonly Func<Task> _onDispose = onDispose;

        public IApiClient ApiClient { get; } = apiClient;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return; 
            await _onDispose.Invoke();
            GC.SuppressFinalize(this);
        }
    }
}

public class ApiClientLockQueue(IApiClient apiClient) : IApiClientLockQueue
{
    private readonly OrderedSemaphore _apiClientLock = new();

    public async Task<IApiClientLockQueue.Lock> GetLock()
    {
        await _apiClientLock.WaitAsync();
        return new IApiClientLockQueue.Lock(apiClient, ReleaseLock);
    }

    private async Task ReleaseLock()
    {
        await Task.Yield();
        _apiClientLock.Release();
    }
}