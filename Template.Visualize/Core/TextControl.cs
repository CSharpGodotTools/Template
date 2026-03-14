#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal sealed class TextControl(LineEdit lineEdit, Func<object, string> stringify) : IVisualControl
{
    public void SetValue(object value)
    {
        lineEdit.Text = stringify(value);
    }

    public Control Control => lineEdit;

    public void SetEditable(bool editable)
    {
        lineEdit.Editable = editable;
    }
}
#endif
