using Godot;

namespace GodotUtils;

/// <summary>
/// Utility helpers for the main window.
/// </summary>
public static class WindowUtils
{
    /// <summary>
    /// Sets the main window title.
    /// </summary>
    public static void SetTitle(string title)
    {
        DisplayServer.WindowSetTitle(title);
    }

    /// <summary>
    /// Gets the center point of the main window in pixels.
    /// </summary>
    public static Vector2 GetCenter()
    {
        Vector2I size = GetSize();
        return new Vector2(size.X / 2f, size.Y / 2f);
    }

    /// <summary>
    /// Gets the main window width in pixels.
    /// </summary>
    public static int GetWidth()
    {
        return GetSize().X;
    }

    /// <summary>
    /// Gets the main window height in pixels.
    /// </summary>
    public static int GetHeight()
    {
        return GetSize().Y;
    }

    private static Vector2I GetSize()
    {
        return DisplayServer.WindowGetSize();
    }
}

public enum WindowMode
{
    Windowed,
    Borderless,
    Fullscreen
}
