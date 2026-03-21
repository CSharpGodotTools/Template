#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualColor(VisualControlContext context)
    {
        Color initialColor = (Color)context.InitialValue!;

        ColorPickerButton colorPickerButton = ColorPickerButtonFactory.Create(initialColor);
        colorPickerButton.Color = initialColor;

        void OnColorChanged(Color color) => context.ValueChanged(color);

        colorPickerButton.ColorChanged += OnColorChanged;
        CleanupOnTreeExited(colorPickerButton, () => colorPickerButton.ColorChanged -= OnColorChanged);

        return new VisualControlInfo(new ColorPickerButtonControl(colorPickerButton));
    }
}

internal sealed class ColorPickerButtonControl : IVisualControl
{
    private readonly ColorPickerButton _colorPickerButton;

    public ColorPickerButtonControl(ColorPickerButton colorPickerButton)
    {
        _colorPickerButton = colorPickerButton;
        _colorPickerButton.TreeEntered += OnTreeEntered;
        _colorPickerButton.TreeExited += OnTreeExited;
    }

    public void SetValue(object value)
    {
        if (value is Color color)
        {
            _colorPickerButton.Color = color;
        }
    }

    public Control Control => _colorPickerButton;

    public void SetEditable(bool editable)
    {
        _colorPickerButton.Disabled = !editable;
    }

    private void OnTreeEntered()
    {
        // Color controls should show their real color, not inherit panel tint.
        Color inheritedTint = GetInheritedTint(_colorPickerButton);
        _colorPickerButton.Modulate = InvertTint(inheritedTint);
    }

    private void OnTreeExited()
    {
        _colorPickerButton.TreeEntered -= OnTreeEntered;
        _colorPickerButton.TreeExited -= OnTreeExited;
    }

    private static Color GetInheritedTint(Node node)
    {
        Color tint = Colors.White;

        Node? current = node.GetParent();
        while (current is CanvasItem canvasItem)
        {
            tint *= canvasItem.Modulate;
            current = canvasItem.GetParent();
        }

        return tint;
    }

    private static Color InvertTint(Color tint)
    {
        float r = tint.R == 0 ? 1f : 1f / tint.R;
        float g = tint.G == 0 ? 1f : 1f / tint.G;
        float b = tint.B == 0 ? 1f : 1f / tint.B;
        return new Color(r, g, b, 1f);
    }
}
#endif
