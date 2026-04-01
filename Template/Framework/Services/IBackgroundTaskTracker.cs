using System;
using System.Threading;
using System.Threading.Tasks;

namespace __TEMPLATE__;

/// <summary>
/// Tracks background tasks and coordinates cancellation during shutdown.
/// </summary>
public interface IBackgroundTaskTracker : IDisposable
{
    /// <summary>
    /// Cancellation token signaled when shutdown starts.
    /// </summary>
    CancellationToken ShutdownToken { get; }

    /// <summary>
    /// Starts and tracks a background task using <see cref="ShutdownToken"/>.
    /// </summary>
    /// <param name="taskFactory">Factory that creates the task.</param>
    /// <param name="taskName">Task name for diagnostics.</param>
    void Run(Func<CancellationToken, Task> taskFactory, string taskName);

    /// <summary>
    /// Tracks an already-created background task.
    /// </summary>
    /// <param name="task">Task to track.</param>
    /// <param name="taskName">Task name for diagnostics.</param>
    void Track(Task task, string taskName);

    /// <summary>
    /// Waits for all currently tracked tasks.
    /// </summary>
    /// <returns>Task that completes when tracked tasks finish.</returns>
    Task WaitForAllAsync();

    /// <summary>
    /// Signals shutdown and waits for task completion.
    /// </summary>
    /// <returns>Task that completes when shutdown tracking finishes.</returns>
    Task ShutdownAsync();
}
