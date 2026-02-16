using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for margin containers.
/// </summary>
public static class MarginContainerFactory
{
    /// <summary>
    /// Creates a margin container with uniform padding.
    /// </summary>
    public static MarginContainer Create(int padding)
    {
        return Create(padding, padding, padding, padding);
    }

    /// <summary>
    /// Creates a margin container with per-side padding.
    /// </summary>
    public static MarginContainer Create(int left, int right, int top, int bottom)
    {
        MarginContainer container = new();
        container.SetMarginLeft(left);
        container.SetMarginRight(right);
        container.SetMarginTop(top);
        container.SetMarginBottom(bottom);
        return container;
    }
}
