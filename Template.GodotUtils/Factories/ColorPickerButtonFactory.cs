using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for color picker buttons.
/// </summary>
public static class ColorPickerButtonFactory
{
    private const int DefaultButtonSize = 30;

    /// <summary>
    /// Creates a <see cref="ColorPickerButton"/> with <paramref name="defaultColor"/>.
    /// </summary>
    /// <param name="defaultColor">Initial picker color.</param>
    /// <returns>Configured color picker button.</returns>
    public static ColorPickerButton Create(Color defaultColor)
    {
        ColorPickerButton button = new()
        {
            CustomMinimumSize = Vector2.One * DefaultButtonSize
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
