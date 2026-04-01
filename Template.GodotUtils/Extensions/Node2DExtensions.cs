using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for Node2D.
/// </summary>
public static class Node2DExtensions
{
    /// <summary>
    /// Sets the color of the given node only.
    /// </summary>
    /// <param name="node">Node to tint.</param>
    /// <param name="color">Tint color.</param>
    public static void SetColor(this Node2D node, Color color)
    {
        node.SelfModulate = color;
    }

    /// <summary>
    /// Recursively sets the color of the node and all its children.
    /// </summary>
    /// <param name="node">Node subtree root to tint.</param>
    /// <param name="color">Tint color.</param>
    public static void SetColorRecursive(this Node2D node, Color color)
    {
        node.Modulate = color;
    }
}
