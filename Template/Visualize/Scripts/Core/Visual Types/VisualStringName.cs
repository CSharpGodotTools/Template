using Godot;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualStringName(VisualControlContext context)
    {
        StringName stringName = (StringName)context.InitialValue;
        string initialText = stringName != null ? stringName.ToString() : string.Empty;

        LineEdit lineEdit = new() { Text = initialText };
        lineEdit.TextChanged += text => context.ValueChanged(new StringName(text));

        return new VisualControlInfo(new LineEditControl(lineEdit));
    }
}
