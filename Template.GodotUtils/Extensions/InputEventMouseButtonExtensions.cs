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
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the event represents wheel-up input.</returns>
    public static bool IsWheelUp(this InputEventMouseButton @event) => IsZoomIn(@event);

    /// <summary>
    /// Returns true when the mouse wheel is scrolled down.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the event represents wheel-down input.</returns>
    public static bool IsWheelDown(this InputEventMouseButton @event) => IsZoomOut(@event);

    /// <summary>
    /// Returns true when the mouse wheel is scrolled up.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the event represents a zoom-in input.</returns>
    public static bool IsZoomIn(this InputEventMouseButton @event) => @event.IsPressed(MouseButton.WheelUp);

    /// <summary>
    /// Returns true when the mouse wheel is scrolled down.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the event represents a zoom-out input.</returns>
    public static bool IsZoomOut(this InputEventMouseButton @event) => @event.IsPressed(MouseButton.WheelDown);

    // Pressed
    /// <summary>
    /// Returns true when the left mouse button is pressed.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the left button is currently pressed.</returns>
    public static bool IsLeftClickPressed(this InputEventMouseButton @event) => @event.IsPressed(MouseButton.Left);

    /// <summary>
    /// Returns true when the left mouse button is just pressed.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the left button has just been pressed.</returns>
    public static bool IsLeftClickJustPressed(this InputEventMouseButton @event) => @event.IsJustPressed(MouseButton.Left);

    /// <summary>
    /// Returns true when the right mouse button is pressed.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the right button is currently pressed.</returns>
    public static bool IsRightClickPressed(this InputEventMouseButton @event) => @event.IsPressed(MouseButton.Right);

    /// <summary>
    /// Returns true when the right mouse button is just pressed.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the right button has just been pressed.</returns>
    public static bool IsRightClickJustPressed(this InputEventMouseButton @event) => @event.IsJustPressed(MouseButton.Right);

    // Released
    /// <summary>
    /// Returns true when the left mouse button is released.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the left button is currently released.</returns>
    public static bool IsLeftClickReleased(this InputEventMouseButton @event) => @event.IsReleased(MouseButton.Left);

    /// <summary>
    /// Returns true when the left mouse button is just released.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the left button has just been released.</returns>
    public static bool IsLeftClickJustReleased(this InputEventMouseButton @event) => @event.IsJustReleased(MouseButton.Left);

    /// <summary>
    /// Returns true when the right mouse button is released.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the right button is currently released.</returns>
    public static bool IsRightClickReleased(this InputEventMouseButton @event) => @event.IsReleased(MouseButton.Right);

    /// <summary>
    /// Returns true when the right mouse button is just released.
    /// </summary>
    /// <param name="event">Mouse button event to evaluate.</param>
    /// <returns><see langword="true"/> when the right button has just been released.</returns>
    public static bool IsRightClickJustReleased(this InputEventMouseButton @event) => @event.IsJustReleased(MouseButton.Right);

    // Helper Functions
    /// <summary>
    /// Returns whether the event represents a pressed state for the requested mouse button.
    /// </summary>
    /// <param name="event">Mouse button input event.</param>
    /// <param name="button">Button index to test.</param>
    /// <returns><see langword="true"/> when the button is pressed in this event.</returns>
    private static bool IsPressed(this InputEventMouseButton @event, MouseButton button) => @event.ButtonIndex == button && @event.Pressed;

    /// <summary>
    /// Returns whether the event is an initial press for the requested mouse button.
    /// </summary>
    /// <param name="event">Mouse button input event.</param>
    /// <param name="button">Button index to test.</param>
    /// <returns><see langword="true"/> when the button was pressed and the event is not echoed.</returns>
    private static bool IsJustPressed(this InputEventMouseButton @event, MouseButton button) => @event.IsPressed(button) && !@event.IsEcho();

    /// <summary>
    /// Returns whether the event represents a released state for the requested mouse button.
    /// </summary>
    /// <param name="event">Mouse button input event.</param>
    /// <param name="button">Button index to test.</param>
    /// <returns><see langword="true"/> when the button is released in this event.</returns>
    private static bool IsReleased(this InputEventMouseButton @event, MouseButton button) => @event.ButtonIndex == button && !@event.Pressed;

    /// <summary>
    /// Returns whether the event is an initial release for the requested mouse button.
    /// </summary>
    /// <param name="event">Mouse button input event.</param>
    /// <param name="button">Button index to test.</param>
    /// <returns><see langword="true"/> when the button was released and the event is not echoed.</returns>
    private static bool IsJustReleased(this InputEventMouseButton @event, MouseButton button) => @event.IsReleased(button) && !@event.IsEcho();
}
