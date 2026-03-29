using Godot;
using System;
using System.Threading.Tasks;

namespace GodotUtils;

public static class TaskUtils
{
    /// <summary>
    /// Runs a task and logs any errors, intended for fire-and-forget usage.
    /// <para>For example:</para>
    /// <code>TaskUtils.FireAndForget(() =&gt; ExitGame());</code>
    /// </summary>
    public static void FireAndForget(this Func<Task> task)
    {
        if (task == null)
            return;

        _ = FireAndForgetInternal(task);
    }

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
