using Godot;
using GodotUtils;
using System;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualEnum(Type type, VisualControlContext context)
    {
        GOptionButtonEnum optionButton = new(type);
        optionButton.Select(context.InitialValue);
        optionButton.OnItemSelected += item => context.ValueChanged(item);

        return new VisualControlInfo(new OptionButtonEnumControl(optionButton));
    }
}

public class OptionButtonEnumControl(GOptionButtonEnum optionButton) : IVisualControl
{
    public void SetValue(object value)
    {
        optionButton.Select(value);
    }

    public Control Control => optionButton.Control;

    public void SetEditable(bool editable)
    {
        optionButton.Control.Disabled = !editable;
    }
}
