#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Color visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a color picker control for <see cref="Color"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created color-control info.</returns>
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

/// <summary>
/// Visual-control wrapper for <see cref="ColorPickerButton"/>.
/// </summary>
internal sealed class ColorPickerButtonControl : IVisualControl
{
    private readonly ColorPickerButton _colorPickerButton;

    /// <summary>
    /// Initializes the control wrapper and hooks tree events.
    /// </summary>
    /// <param name="colorPickerButton">Color picker button to wrap.</param>
    public ColorPickerButtonControl(ColorPickerButton colorPickerButton)
    {
        _colorPickerButton = colorPickerButton;
        _colorPickerButton.TreeEntered += OnTreeEntered;
        _colorPickerButton.TreeExited += OnTreeExited;
    }

    /// <summary>
    /// Updates the picker color from the provided value.
    /// </summary>
    /// <param name="value">Incoming value.</param>
    public void SetValue(object value)
    {
        // Only apply updates when a color value is provided.
        if (value is Color color)
        {
            _colorPickerButton.Color = color;
        }
    }

    /// <summary>
    /// Gets the underlying color picker control.
    /// </summary>
    public Control Control => _colorPickerButton;

    /// <summary>
    /// Toggles editability for the color picker.
    /// </summary>
    /// <param name="editable">Whether the control is editable.</param>
    public void SetEditable(bool editable)
    {
        _colorPickerButton.Disabled = !editable;
    }

    /// <summary>
    /// Recomputes control modulation when the picker enters the tree to keep the displayed color accurate.
    /// </summary>
    private void OnTreeEntered()
    {
        // Color controls should show their real color, not inherit panel tint.
        Color inheritedTint = GetInheritedTint(_colorPickerButton);
        _colorPickerButton.Modulate = InvertTint(inheritedTint);
    }

    /// <summary>
    /// Detaches temporary tree event handlers when the picker exits the tree.
    /// </summary>
    private void OnTreeExited()
    {
        _colorPickerButton.TreeEntered -= OnTreeEntered;
        _colorPickerButton.TreeExited -= OnTreeExited;
    }

    /// <summary>
    /// Accumulates inherited modulation tint from parent <see cref="CanvasItem"/> nodes.
    /// </summary>
    /// <param name="node">Node whose parent chain is inspected.</param>
    /// <returns>Combined tint color inherited from ancestors.</returns>
    private static Color GetInheritedTint(Node node)
    {
        Color tint = Colors.White;

        Node? current = node.GetParent();
        // Walk up the tree accumulating modulate tints.
        while (current is CanvasItem canvasItem)
        {
            tint *= canvasItem.Modulate;
            current = canvasItem.GetParent();
        }

        return tint;
    }

    /// <summary>
    /// Inverts tint channels so the picker can neutralize inherited modulation.
    /// </summary>
    /// <param name="tint">Tint to invert.</param>
    /// <returns>Inverted tint with alpha forced to 1.</returns>
    private static Color InvertTint(Color tint)
    {
        // Avoid divide-by-zero by falling back to 1 for zero channels.
        float r = tint.R == 0 ? 1f : 1f / tint.R;
        float g = tint.G == 0 ? 1f : 1f / tint.G;
        float b = tint.B == 0 ? 1f : 1f / tint.B;
        return new Color(r, g, b, 1f);
    }
}
#endif
