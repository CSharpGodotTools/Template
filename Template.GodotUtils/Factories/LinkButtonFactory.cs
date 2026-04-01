using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for link buttons.
/// </summary>
public static class LinkButtonFactory
{
    /// <summary>
    /// Creates a link button with the provided text and font size.
    /// </summary>
    /// <param name="text">Button label and URI value.</param>
    /// <param name="fontSize">Font size in points.</param>
    /// <returns>Configured link button.</returns>
    public static LinkButton Create(string text, int fontSize = 16)
    {
        LinkButton button = new()
        {
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            Text = text,
            Uri = text
        };

        button.SetFontSize(fontSize);

        return button;
    }
}
