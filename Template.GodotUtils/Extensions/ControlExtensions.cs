using Godot;
using Layout = Godot.Control.LayoutPreset;

namespace GodotUtils;

/// <summary>
/// Extension helpers for configuring common <see cref="Control"/> properties.
/// </summary>
public static class ControlExtensions
{
    /// <summary>
    /// Sets the layout for the specified control.
    /// </summary>
    /// <param name="control">Control to configure.</param>
    /// <param name="layout">Preset layout to apply.</param>
    /// <returns>The same control instance for fluent chaining.</returns>
    /// <remarks>
    /// Applies immediately if inside the tree, otherwise waits for ready.
    /// </remarks>
    public static Control SetLayout(this Control control, Layout layout)
    {
        // Apply immediately when node is already in the scene tree.
        if (control.IsInsideTree())
        {
            control.SetAnchorsAndOffsetsPreset(layout);
        }
        // Otherwise defer until Ready to avoid invalid layout state.
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
    /// <param name="control">Control to configure.</param>
    /// <param name="fontSize">Font size override value.</param>
    /// <returns>The same control instance for fluent chaining.</returns>
    public static Control SetFontSize(this Control control, int fontSize)
    {
        control.AddThemeFontSizeOverride("font_size", fontSize);
        return control;
    }

    /// <summary>
    /// Sets the left margin override.
    /// </summary>
    /// <param name="control">Control to configure.</param>
    /// <param name="padding">Margin value in pixels.</param>
    /// <returns>The same control instance for fluent chaining.</returns>
    public static Control SetMarginLeft(this Control control, int padding)
    {
        return SetMargin(control, "margin_left", padding);
    }

    /// <summary>
    /// Sets the right margin override.
    /// </summary>
    /// <param name="control">Control to configure.</param>
    /// <param name="padding">Margin value in pixels.</param>
    /// <returns>The same control instance for fluent chaining.</returns>
    public static Control SetMarginRight(this Control control, int padding)
    {
        return SetMargin(control, "margin_right", padding);
    }

    /// <summary>
    /// Sets the top margin override.
    /// </summary>
    /// <param name="control">Control to configure.</param>
    /// <param name="padding">Margin value in pixels.</param>
    /// <returns>The same control instance for fluent chaining.</returns>
    public static Control SetMarginTop(this Control control, int padding)
    {
        return SetMargin(control, "margin_top", padding);
    }

    /// <summary>
    /// Sets the bottom margin override.
    /// </summary>
    /// <param name="control">Control to configure.</param>
    /// <param name="padding">Margin value in pixels.</param>
    /// <returns>The same control instance for fluent chaining.</returns>
    public static Control SetMarginBottom(this Control control, int padding)
    {
        return SetMargin(control, "margin_bottom", padding);
    }

    /// <summary>
    /// Applies a theme margin override and returns the same control for fluent chaining.
    /// </summary>
    /// <param name="control">Control receiving the override.</param>
    /// <param name="key">Theme constant key for the margin side.</param>
    /// <param name="padding">Margin value in pixels.</param>
    /// <returns>The same control instance.</returns>
    private static Control SetMargin(Control control, string key, int padding)
    {
        control.AddThemeConstantOverride(key, padding);
        return control;
    }
}
