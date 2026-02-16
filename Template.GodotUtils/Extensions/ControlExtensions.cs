using Godot;
using Layout = Godot.Control.LayoutPreset;

namespace GodotUtils;

public static class ControlExtensions
{
    /// <summary>
    /// Sets the layout for the specified control.
    /// </summary>
    /// <remarks>
    /// Applies immediately if inside the tree, otherwise waits for ready.
    /// </remarks>
    public static Control SetLayout(this Control control, Layout layout)
    {
        if (control.IsInsideTree())
        {
            control.SetAnchorsAndOffsetsPreset(layout);
        }
        else
        {
            control.Ready += OnReady;
            control.TreeExited += OnExitedTree;
        }

        return control;

        void OnReady()
        {
            control.SetAnchorsAndOffsetsPreset(layout);
        }

        void OnExitedTree()
        {
            control.Ready -= OnReady;
            control.TreeExited -= OnExitedTree;
        }
    }

    /// <summary>
    /// Sets the font size override.
    /// </summary>
    public static Control SetFontSize(this Control control, int fontSize)
    {
        control.AddThemeFontSizeOverride("font_size", fontSize);
        return control;
    }

    /// <summary>
    /// Sets the left margin override.
    /// </summary>
    public static Control SetMarginLeft(this Control control, int padding)
    {
        return SetMargin(control, "margin_left", padding);
    }

    /// <summary>
    /// Sets the right margin override.
    /// </summary>
    public static Control SetMarginRight(this Control control, int padding)
    {
        return SetMargin(control, "margin_right", padding);
    }

    /// <summary>
    /// Sets the top margin override.
    /// </summary>
    public static Control SetMarginTop(this Control control, int padding)
    {
        return SetMargin(control, "margin_top", padding);
    }

    /// <summary>
    /// Sets the bottom margin override.
    /// </summary>
    public static Control SetMarginBottom(this Control control, int padding)
    {
        return SetMargin(control, "margin_bottom", padding);
    }

    private static Control SetMargin(Control control, string key, int padding)
    {
        control.AddThemeConstantOverride(key, padding);
        return control;
    }
}
