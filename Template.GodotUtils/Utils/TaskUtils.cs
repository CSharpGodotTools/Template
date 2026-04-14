using Godot;
using System;
using System.Threading.Tasks;

namespace GodotUtils;

/// <summary>
/// Helpers for fire-and-forget task execution with non-fatal exception logging.
/// </summary>
public static class TaskUtils
{
    /// <summary>
    /// Runs a task and logs any errors, intended for fire-and-forget usage.
    /// <para>For example:</para>
    /// <c>TaskUtils.FireAndForget(() =&gt; ExitGame());</c>
    /// </summary>
    /// <param name="task">Task delegate to execute.</param>
    public static void FireAndForget(this Func<Task> task)
    {
        // Ignore null delegates to keep call sites concise.
        if (task == null)
            return;

        _ = FireAndForgetInternal(task);
    }

    /// <summary>
    /// Executes the task and handles expected cancellation and non-fatal failures.
    /// </summary>
    /// <param name="task">Task delegate to execute.</param>
    /// <returns>Task representing the fire-and-forget execution lifecycle.</returns>
    private static async Task FireAndForgetInternal(Func<Task> task)
    {
        try
        {
            await task();
        }
        catch (OperationCanceledException)
        {
            // Expected when the task is canceled by shutdown/dispose flow.
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            GD.PrintErr($"Error: {exception}");
        }
    }
}
