using Godot;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualBool(VisualControlContext context)
    {
        CheckBox checkBox = new() { ButtonPressed = (bool)context.InitialValue };
        checkBox.Toggled += value => context.ValueChanged(value);

        return new VisualControlInfo(new BoolControl(checkBox));
    }
}

public class BoolControl(CheckBox checkBox) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is bool boolValue)
        {
            checkBox.ButtonPressed = boolValue;
        }
    }

    public Control Control => checkBox;

    public void SetEditable(bool editable)
    {
        checkBox.Disabled = !editable;
    }
}
