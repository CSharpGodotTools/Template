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
    public static async void FireAndForget(this Func<Task> task)
    {
        if (task == null)
            return;

        try
        {
            await task();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error: {e}");
        }
    }
}
