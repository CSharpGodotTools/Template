using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace __TEMPLATE__;

internal sealed class BackgroundTaskTracker(ILoggerService logger) : IBackgroundTaskTracker
{
    private readonly ILoggerService _logger = logger;
    private readonly ConcurrentDictionary<int, Task> _tasks = new();
    private readonly CancellationTokenSource _shutdownCts = new();
    private int _shutdownStarted;
    private int _disposed;

    public CancellationToken ShutdownToken => _shutdownCts.Token;

    public void Run(Func<CancellationToken, Task> taskFactory, string taskName)
    {
        ArgumentNullException.ThrowIfNull(taskFactory);

        Task task;
        try
        {
            task = taskFactory(_shutdownCts.Token);
        }
        catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
        {
            return;
        }
        catch (ObjectDisposedException exception)
        {
            _logger.LogErr(exception, $"Failed to start background task '{taskName}'");
            return;
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogErr(exception, $"Failed to start background task '{taskName}'");
            return;
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            _logger.LogErr(exception, $"Failed to start background task '{taskName}'");
            return;
        }

        Track(task, taskName);
    }

    public void Track(Task task, string taskName)
    {
        ArgumentNullException.ThrowIfNull(task);

        string name = string.IsNullOrWhiteSpace(taskName)
            ? "UnnamedBackgroundTask"
            : taskName;

        _tasks[task.Id] = task;
        _ = ObserveTaskAsync(task, name);
    }

    public async Task WaitForAllAsync()
    {
        Task[] snapshot = [.. _tasks.Values];
        if (snapshot.Length == 0)
            return;

        try
        {
            await Task.WhenAll(snapshot).ConfigureAwait(false);
        }
        catch
        {
            // Faults are already observed and logged by ObserveTaskAsync.
        }
    }

    public async Task ShutdownAsync()
    {
        if (Interlocked.CompareExchange(ref _shutdownStarted, 1, 0) == 0)
        {
            _shutdownCts.Cancel();
        }

        await WaitForAllAsync().ConfigureAwait(false);
    }

    private async Task ObserveTaskAsync(Task task, string taskName)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException exception)
        {
            _logger.LogErr(exception, $"Background task '{taskName}' failed");
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogErr(exception, $"Background task '{taskName}' failed");
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            _logger.LogErr(exception, $"Background task '{taskName}' failed");
        }
        finally
        {
            _tasks.TryRemove(task.Id, out _);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        _shutdownCts.Cancel();
        _shutdownCts.Dispose();
    }
}
