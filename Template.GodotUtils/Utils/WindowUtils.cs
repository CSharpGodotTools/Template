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
    /// <param name="title">Window title text.</param>
    public static void SetTitle(string title)
    {
        DisplayServer.WindowSetTitle(title);
    }

    /// <summary>
    /// Gets the center point of the main window in pixels.
    /// </summary>
    /// <returns>Window center position in pixels.</returns>
    public static Vector2 GetCenter()
    {
        Vector2I size = GetSize();
        return new Vector2(size.X / 2f, size.Y / 2f);
    }

    /// <summary>
    /// Gets the main window width in pixels.
    /// </summary>
    /// <returns>Window width in pixels.</returns>
    public static int GetWidth()
    {
        return GetSize().X;
    }

    /// <summary>
    /// Gets the main window height in pixels.
    /// </summary>
    /// <returns>Window height in pixels.</returns>
    public static int GetHeight()
    {
        return GetSize().Y;
    }

    /// <summary>
    /// Gets the current main window size.
    /// </summary>
    /// <returns>Window size in pixels.</returns>
    private static Vector2I GetSize()
    {
        return DisplayServer.WindowGetSize();
    }
}

/// <summary>
/// High-level window mode options.
/// </summary>
public enum WindowMode
{
    Windowed,
    Borderless,
    Fullscreen
}
