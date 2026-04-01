#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Composite visual-control adapter for container-based controls with nested inputs.
/// </summary>
/// <param name="vboxContainer">Root VBox container.</param>
/// <param name="setValueAction">Optional refresh callback when value is set.</param>
/// <param name="setEditableAction">Optional callback before recursive editable updates.</param>
internal sealed class VBoxContainerControl(
    VBoxContainer vboxContainer,
    Action<object>? setValueAction = null,
    Action<bool>? setEditableAction = null) : IVisualControl
{
    /// <summary>
    /// Invokes custom value refresh logic for composite controls.
    /// </summary>
    /// <param name="value">Value payload used by the composite control.</param>
    public void SetValue(object value)
    {
        // Collection controls provide a custom refresh action through this callback.
        setValueAction?.Invoke(value);
    }

    /// <summary>
    /// Root composite control.
    /// </summary>
    public Control Control => vboxContainer;

    /// <summary>
    /// Toggles editability for this control and all nested editable descendants.
    /// </summary>
    /// <param name="editable">True to allow edits.</param>
    public void SetEditable(bool editable)
    {
        // Allow composite controls to update button states before the recursive pass.
        setEditableAction?.Invoke(editable);
        ApplyEditableRecursive(vboxContainer, editable);
    }

    /// <summary>
    /// Applies editable state to this control and all control descendants.
    /// </summary>
    /// <param name="control">Root control to update.</param>
    /// <param name="editable">Whether editable controls should accept user input.</param>
    private static void ApplyEditableRecursive(Control control, bool editable)
    {
        // Composite controls contain multiple input nodes; lock or unlock all descendants.
        if (control is BaseButton button)
        {
            button.Disabled = !editable;
        }

        // Spin boxes expose editability through the Editable property.
        if (control is SpinBox spinBox)
        {
            spinBox.Editable = editable;
        }

        // Line edits expose editability through the Editable property.
        if (control is LineEdit lineEdit)
        {
            lineEdit.Editable = editable;
        }

        foreach (Node child in control.GetChildren())
        {
            // Recurse only into child nodes that are controls.
            if (child is Control childControl)
            {
                ApplyEditableRecursive(childControl, editable);
            }
        }
    }
}
#endif
