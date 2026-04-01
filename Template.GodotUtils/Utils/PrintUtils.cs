using Godot;

namespace GodotUtils;

/// <summary>
/// Minimal print helpers for warnings and diagnostic output.
/// </summary>
public static class PrintUtils
{
    /// <summary>
    /// Prints a warning with a yellow highlight and pushes it to the editor warnings.
    /// </summary>
    /// <param name="message">Warning message to display.</param>
    public static void Warning(object message)
    {
        GD.PrintRich($"[color=yellow]{message}[/color]");
        GD.PushWarning(message);
    }
}
