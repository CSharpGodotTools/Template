using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for labels.
/// </summary>
public static class LabelFactory
{
    /// <summary>
    /// Creates a centered label with the provided text and font size.
    /// </summary>
    public static Label Create(string text = "", int fontSize = 16)
    {
        Label label = new()
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        label.SetFontSize(fontSize);

        return label;
    }
}
