using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for color picker buttons.
/// </summary>
public static class ColorPickerButtonFactory
{
    /// <summary>
    /// Creates a <see cref="ColorPickerButton"/> with <paramref name="defaultColor"/>.
    /// </summary>
    public static ColorPickerButton Create(Color defaultColor)
    {
        ColorPickerButton button = new()
        {
            CustomMinimumSize = Vector2.One * 30
        };

        button.PickerCreated += OnPickerCreated;
        button.PopupClosed += OnPopupClosed;
        button.TreeExited += OnExitedTree;

        return button;

        void OnPickerCreated()
        {
            button.PickerCreated -= OnPickerCreated;

            ColorPicker picker = button.GetPicker();
            picker.Color = defaultColor;
        }

        void OnPopupClosed() => button.ReleaseFocus();

        void OnExitedTree()
        {
            button.PopupClosed -= OnPopupClosed;
            button.TreeExited -= OnExitedTree;
        }
    }
}
