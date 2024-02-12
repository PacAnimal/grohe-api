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
    private readonly Queue<TaskCompletionSource> _queue = new();

    public async Task<IApiClientLockQueue.Lock> GetLock()
    {
        var driverLock = new IApiClientLockQueue.Lock(apiClient, ReleaseLock);
        var tcs = new TaskCompletionSource();
        lock (_queue)
        {
            _queue.Enqueue(tcs);
            if (_queue.Peek() == tcs)
            {
                tcs.SetResult();
            }
        }
        await tcs.Task;
        return driverLock;
    }

    private async Task ReleaseLock()
    {
        await Task.Yield();
        lock (_queue)
        {
            _queue.Dequeue();
            if (_queue.TryPeek(out var tcs))
            {
                tcs.SetResult();
            }
        }
    }
}