using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for key input events.
/// </summary>
public static class InputEventKeyExtensions
{
    /// <summary>
    /// Returns true when the key is pressed this frame without repeat.
    /// </summary>
    /// <param name="v">Input key event to evaluate.</param>
    /// <param name="key">Target key to compare against.</param>
    /// <returns><see langword="true"/> when the key is newly pressed.</returns>
    public static bool IsJustPressed(this InputEventKey v, Key key)
    {
        return v.Keycode == key && v.Pressed && !v.Echo;
    }

    /// <summary>
    /// Returns true when the key is released this frame without repeat.
    /// </summary>
    /// <param name="v">Input key event to evaluate.</param>
    /// <param name="key">Target key to compare against.</param>
    /// <returns><see langword="true"/> when the key is newly released.</returns>
    public static bool IsJustReleased(this InputEventKey v, Key key)
    {
        return v.Keycode == key && !v.Pressed && !v.Echo;
    }

    /// <summary>
    /// Converts to a human readable key string (e.g. "Ctrl + Shift + E").
    /// </summary>
    /// <param name="v">Input key event to convert.</param>
    /// <returns>Formatted key string including active modifiers.</returns>
    public static string Readable(this InputEventKey v)
    {
        // Fall back to physical keycode when logical keycode is not set.
        Key keyWithModifiers = v.Keycode == Key.None ?
            v.GetPhysicalKeycodeWithModifiers() :
            v.GetKeycodeWithModifiers();

        return OS.GetKeycodeString(keyWithModifiers).Replace("+", " + ");
    }
}
