using Godot;

namespace GodotUtils;

public static class PrintUtils
{
    /// <summary>
    /// Prints a warning with a yellow highlight and pushes it to the editor warnings.
    /// </summary>
    public static void Warning(object message)
    {
        GD.PrintRich($"[color=yellow]{message}[/color]");
        GD.PushWarning(message);
    }
}
