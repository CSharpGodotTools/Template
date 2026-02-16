#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualColor(VisualControlContext context)
    {
        Color initialColor = (Color)context.InitialValue;

        ColorPickerButton colorPickerButton = ColorPickerButtonFactory.Create(initialColor);
        colorPickerButton.ColorChanged += color => context.ValueChanged(color);

        return new VisualControlInfo(new ColorPickerButtonControl(colorPickerButton));
    }
}

internal sealed class ColorPickerButtonControl(ColorPickerButton colorPickerButton) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is Color color)
        {
            colorPickerButton.Color = color;
        }
    }

    public Control Control => colorPickerButton;

    public void SetEditable(bool editable)
    {
        colorPickerButton.Disabled = !editable;
    }
}
#endif
