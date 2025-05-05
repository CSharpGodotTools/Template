using Godot;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualNodePath(VisualControlContext context)
    {
        NodePath nodePath = (NodePath)context.InitialValue;
        string initialText = nodePath != null ? nodePath.ToString() : string.Empty;

        LineEdit lineEdit = new() { Text = initialText };
        lineEdit.TextChanged += text => context.ValueChanged(new NodePath(text));

        return new VisualControlInfo(new LineEditControl(lineEdit));
    }
}
