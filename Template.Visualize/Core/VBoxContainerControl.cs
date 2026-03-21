#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal sealed class VBoxContainerControl(
    VBoxContainer vboxContainer,
    Action<object>? setValueAction = null,
    Action<bool>? setEditableAction = null) : IVisualControl
{
    public void SetValue(object value)
    {
        // Collection controls provide a custom refresh action through this callback.
        setValueAction?.Invoke(value);
    }

    public Control Control => vboxContainer;

    public void SetEditable(bool editable)
    {
        // Allow composite controls to update button states before the recursive pass.
        setEditableAction?.Invoke(editable);
        ApplyEditableRecursive(vboxContainer, editable);
    }

    /// <summary>
    /// Applies editable state to this control and all control descendants.
    /// </summary>
    private static void ApplyEditableRecursive(Control control, bool editable)
    {
        // Composite controls contain multiple input nodes; lock or unlock all descendants.
        if (control is BaseButton button)
        {
            button.Disabled = !editable;
        }

        if (control is SpinBox spinBox)
        {
            spinBox.Editable = editable;
        }

        if (control is LineEdit lineEdit)
        {
            lineEdit.Editable = editable;
        }

        foreach (Node child in control.GetChildren())
        {
            if (child is Control childControl)
            {
                ApplyEditableRecursive(childControl, editable);
            }
        }
    }
}
#endif
