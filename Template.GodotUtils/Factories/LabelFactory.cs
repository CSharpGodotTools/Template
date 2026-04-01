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
    /// <param name="text">Initial label text.</param>
    /// <param name="fontSize">Font size in points.</param>
    /// <returns>Configured label instance.</returns>
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
