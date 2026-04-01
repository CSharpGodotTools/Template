#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Contract for controls that can display values and toggle editability.
/// </summary>
internal interface IVisualControl
{
    /// <summary>
    /// Updates the rendered control value.
    /// </summary>
    /// <param name="value">New value to display.</param>
    void SetValue(object value);

    /// <summary>
    /// Root control node rendered in the UI.
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Enables or disables user editing.
    /// </summary>
    /// <param name="editable">True to allow edits.</param>
    void SetEditable(bool editable);
}
#endif
