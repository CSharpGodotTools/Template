#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Visual-control adapter for <see cref="LineEdit"/> values.
/// </summary>
/// <param name="lineEdit">Underlying line-edit control.</param>
/// <param name="stringify">Value-to-text formatter.</param>
internal sealed class TextControl(LineEdit lineEdit, Func<object, string> stringify) : IVisualControl
{
    /// <summary>
    /// Updates the line-edit text from the supplied value.
    /// </summary>
    /// <param name="value">Value to display.</param>
    public void SetValue(object value)
    {
        lineEdit.Text = stringify(value);
    }

    /// <summary>
    /// Root line-edit control.
    /// </summary>
    public Control Control => lineEdit;

    /// <summary>
    /// Enables or disables text editing.
    /// </summary>
    /// <param name="editable">True to allow edits.</param>
    public void SetEditable(bool editable)
    {
        lineEdit.Editable = editable;
    }
}
#endif
