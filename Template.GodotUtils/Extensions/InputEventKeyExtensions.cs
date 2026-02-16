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
    public static bool IsJustPressed(this InputEventKey v, Key key)
    {
        return v.Keycode == key && v.Pressed && !v.Echo;
    }

    /// <summary>
    /// Returns true when the key is released this frame without repeat.
    /// </summary>
    public static bool IsJustReleased(this InputEventKey v, Key key)
    {
        return v.Keycode == key && !v.Pressed && !v.Echo;
    }

    /// <summary>
    /// Converts to a human readable key string (e.g. "Ctrl + Shift + E").
    /// </summary>
    public static string Readable(this InputEventKey v)
    {
        // If Keycode is not set then use PhysicalKeycode.
        Key keyWithModifiers = v.Keycode == Key.None ?
            v.GetPhysicalKeycodeWithModifiers() :
            v.GetKeycodeWithModifiers();

        return OS.GetKeycodeString(keyWithModifiers).Replace("+", " + ");
    }
}
