using Godot;
using GodotUtils;

namespace __TEMPLATE__;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualColor(VisualControlContext context)
    {
        Color initialColor = (Color)context.InitialValue;

        GColorPickerButton colorPickerButton = new(initialColor);
        colorPickerButton.OnColorChanged += color => context.ValueChanged(color);

        return new VisualControlInfo(new ColorPickerButtonControl(colorPickerButton));
    }
}

public class ColorPickerButtonControl(GColorPickerButton colorPickerButton) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is Color color)
        {
            colorPickerButton.Internal.Color = color;
        }
    }

    public Control Control => colorPickerButton.Internal;

    public void SetEditable(bool editable)
    {
        colorPickerButton.Internal.Disabled = !editable;
    }
}
