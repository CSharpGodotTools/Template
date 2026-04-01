using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace __TEMPLATE__;

/// <summary>
/// Tracks background tasks, logs failures, and coordinates shutdown cancellation.
/// </summary>
/// <param name="logger">Logger used for task lifecycle failures.</param>
internal sealed class BackgroundTaskTracker(ILoggerService logger) : IBackgroundTaskTracker
{
    private readonly ILoggerService _logger = logger;
    private readonly ConcurrentDictionary<int, Task> _tasks = new();
    private readonly CancellationTokenSource _shutdownCts = new();
    private int _shutdownStarted;
    private int _disposed;

    /// <summary>
    /// Cancellation token that signals application shutdown.
    /// </summary>
    public CancellationToken ShutdownToken => _shutdownCts.Token;

    /// <summary>
    /// Starts a background task using the shutdown token and tracks it.
    /// </summary>
    /// <param name="taskFactory">Factory that creates the task.</param>
    /// <param name="taskName">Task name used in diagnostics.</param>
    public void Run(Func<CancellationToken, Task> taskFactory, string taskName)
    {
        ArgumentNullException.ThrowIfNull(taskFactory);

        Task task;
        try
        {
            task = taskFactory(_shutdownCts.Token);
        }
        // Shutdown-triggered cancellations are expected and should stay silent.
        catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
        {
            return;
        }
        // Non-fatal startup failures are logged and swallowed.
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            _logger.LogErr(exception, $"Failed to start background task '{taskName}'");
            return;
        }

        Track(task, taskName);
    }

    /// <summary>
    /// Registers an existing background task for observation and shutdown waiting.
    /// </summary>
    /// <param name="task">Task to track.</param>
    /// <param name="taskName">Task name used in diagnostics.</param>
    public void Track(Task task, string taskName)
    {
        ArgumentNullException.ThrowIfNull(task);

        // Ensure logs always include a non-empty task name.
        string name = string.IsNullOrWhiteSpace(taskName)
            ? "UnnamedBackgroundTask"
            : taskName;

        _tasks[task.Id] = task;
        _ = ObserveTaskAsync(task, name);
    }

    /// <summary>
    /// Waits for all currently tracked tasks to finish.
    /// </summary>
    /// <returns>Task that completes when tracked tasks are done.</returns>
    public async Task WaitForAllAsync()
    {
        // Snapshot prevents concurrent dictionary mutations during wait setup.
        Task[] snapshot = [.. _tasks.Values];

        // Return immediately when there are no tracked tasks.
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

    /// <summary>
    /// Cancels shutdown token once and waits for tracked tasks to complete.
    /// </summary>
    /// <returns>Task that completes when shutdown observation finishes.</returns>
    public async Task ShutdownAsync()
    {
        // Cancel only once when multiple callers race during shutdown.
        if (Interlocked.CompareExchange(ref _shutdownStarted, 1, 0) == 0)
        {
            _shutdownCts.Cancel();
        }

        await WaitForAllAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Observes a tracked task to log failures and remove it from tracking.
    /// </summary>
    /// <param name="task">Task to observe.</param>
    /// <param name="taskName">Task name used in diagnostics.</param>
    /// <returns>Observer task.</returns>
    private async Task ObserveTaskAsync(Task task, string taskName)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        // Shutdown-triggered cancellations are expected and should stay silent.
        catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
        {
        }
        // Non-fatal task failures are logged and swallowed.
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            _logger.LogErr(exception, $"Background task '{taskName}' failed");
        }
        finally
        {
            _tasks.TryRemove(task.Id, out _);
        }
    }

    /// <summary>
    /// Cancels shutdown token and releases resources.
    /// </summary>
    public void Dispose()
    {
        // Dispose logic must run only once.
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        _shutdownCts.Cancel();
        _shutdownCts.Dispose();
    }
}
