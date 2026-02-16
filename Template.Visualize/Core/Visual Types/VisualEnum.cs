#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualEnum(Type type, VisualControlContext context)
    {
        if (!type.IsEnum)
            throw new ArgumentException("type must be an enum");

        OptionButton optionButton = new()
        {
            Alignment = HorizontalAlignment.Center
        };

        foreach (object item in Enum.GetValues(type))
        {
            optionButton.AddItem(VisualText.ToSpacedName(item.ToString()));
        }

        optionButton.ItemSelected += index =>
        {
            object selectedValue = Enum.GetValues(type).GetValue(index);
            context.ValueChanged(selectedValue);
            optionButton.ReleaseFocus();
        };

        void Select(object value)
        {
            int selectedIndex = Array.IndexOf(Enum.GetValues(type), value);
            optionButton.Select(selectedIndex);
        }

        Select(context.InitialValue);

        return new VisualControlInfo(new OptionButtonEnumControl(optionButton, Select));
    }
}

internal sealed class OptionButtonEnumControl(OptionButton optionButton, Action<object> select) : IVisualControl
{
    public Control Control => optionButton;

    public void SetValue(object value)
    {
        select(value);
    }

    public void SetEditable(bool editable)
    {
        optionButton.Disabled = !editable;
    }
}
#endif
