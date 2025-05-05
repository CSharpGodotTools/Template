using Godot;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualString(VisualControlContext context)
    {
        LineEdit lineEdit = new() { Text = context.InitialValue.ToString() };
        lineEdit.TextChanged += text => context.ValueChanged(text);

        return new VisualControlInfo(new LineEditControl(lineEdit));
    }
}

public class LineEditControl(LineEdit lineEdit) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is string text)
        {
            lineEdit.Text = text;
        }
    }

    public Control Control => lineEdit;

    public void SetEditable(bool editable)
    {
        lineEdit.Editable = editable;
    }
}
