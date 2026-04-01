#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Boolean visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a checkbox control for boolean values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created boolean-control info.</returns>
    private static VisualControlInfo VisualBool(VisualControlContext context)
    {
        CheckBox checkBox = new() { ButtonPressed = (bool)context.InitialValue! };

        void OnToggled(bool value) => context.ValueChanged(value);

        checkBox.Toggled += OnToggled;
        CleanupOnTreeExited(checkBox, () => checkBox.Toggled -= OnToggled);

        return new VisualControlInfo(new BoolControl(checkBox));
    }
}

/// <summary>
/// Wraps a checkbox as a visual-control implementation.
/// </summary>
/// <param name="checkBox">Checkbox instance to manage.</param>
internal sealed class BoolControl(CheckBox checkBox) : IVisualControl
{
    /// <summary>
    /// Updates the checkbox state from the provided value.
    /// </summary>
    /// <param name="value">Incoming value.</param>
    public void SetValue(object value)
    {
        // Only apply updates when a boolean value is provided.
        if (value is bool boolValue)
        {
            checkBox.ButtonPressed = boolValue;
        }
    }

    /// <summary>
    /// Gets the underlying checkbox control.
    /// </summary>
    public Control Control => checkBox;

    /// <summary>
    /// Toggles editability for the checkbox.
    /// </summary>
    /// <param name="editable">Whether the checkbox is editable.</param>
    public void SetEditable(bool editable)
    {
        checkBox.Disabled = !editable;
    }
}
#endif
