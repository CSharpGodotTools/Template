using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for mouse button events.
/// </summary>
public static class InputEventMouseButtonExtensions
{
    // Zoom
    /// <summary>
    /// Returns true when the mouse wheel is scrolled up.
    /// </summary>
    public static bool IsWheelUp(this InputEventMouseButton @event) => IsZoomIn(@event);

    /// <summary>
    /// Returns true when the mouse wheel is scrolled down.
    /// </summary>
    public static bool IsWheelDown(this InputEventMouseButton @event) => IsZoomOut(@event);

    /// <summary>
    /// Returns true when the mouse wheel is scrolled up.
    /// </summary>
    public static bool IsZoomIn(this InputEventMouseButton @event) => @event.IsPressed(MouseButton.WheelUp);

    /// <summary>
    /// Returns true when the mouse wheel is scrolled down.
    /// </summary>
    public static bool IsZoomOut(this InputEventMouseButton @event) => @event.IsPressed(MouseButton.WheelDown);

    // Pressed
    /// <summary>
    /// Returns true when the left mouse button is pressed.
    /// </summary>
    public static bool IsLeftClickPressed(this InputEventMouseButton @event) => @event.IsPressed(MouseButton.Left);

    /// <summary>
    /// Returns true when the left mouse button is just pressed.
    /// </summary>
    public static bool IsLeftClickJustPressed(this InputEventMouseButton @event) => @event.IsJustPressed(MouseButton.Left);

    /// <summary>
    /// Returns true when the right mouse button is pressed.
    /// </summary>
    public static bool IsRightClickPressed(this InputEventMouseButton @event) => @event.IsPressed(MouseButton.Right);

    /// <summary>
    /// Returns true when the right mouse button is just pressed.
    /// </summary>
    public static bool IsRightClickJustPressed(this InputEventMouseButton @event) => @event.IsJustPressed(MouseButton.Right);

    // Released
    /// <summary>
    /// Returns true when the left mouse button is released.
    /// </summary>
    public static bool IsLeftClickReleased(this InputEventMouseButton @event) => @event.IsReleased(MouseButton.Left);

    /// <summary>
    /// Returns true when the left mouse button is just released.
    /// </summary>
    public static bool IsLeftClickJustReleased(this InputEventMouseButton @event) => @event.IsJustReleased(MouseButton.Left);

    /// <summary>
    /// Returns true when the right mouse button is released.
    /// </summary>
    public static bool IsRightClickReleased(this InputEventMouseButton @event) => @event.IsReleased(MouseButton.Right);

    /// <summary>
    /// Returns true when the right mouse button is just released.
    /// </summary>
    public static bool IsRightClickJustReleased(this InputEventMouseButton @event) => @event.IsJustReleased(MouseButton.Right);

    // Helper Functions
    private static bool IsPressed(this InputEventMouseButton @event, MouseButton button) => @event.ButtonIndex == button && @event.Pressed;
    private static bool IsJustPressed(this InputEventMouseButton @event, MouseButton button) => @event.IsPressed(button) && !@event.IsEcho();
    private static bool IsReleased(this InputEventMouseButton @event, MouseButton button) => @event.ButtonIndex == button && !@event.Pressed;
    private static bool IsJustReleased(this InputEventMouseButton @event, MouseButton button) => @event.IsReleased(button) && !@event.IsEcho();
}
