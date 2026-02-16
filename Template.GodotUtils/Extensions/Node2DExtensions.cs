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
    public static void SetColor(this Node2D node, Color color)
    {
        node.SelfModulate = color;
    }

    /// <summary>
    /// Recursively sets the color of the node and all its children.
    /// </summary>
    public static void SetColorRecursive(this Node2D node, Color color)
    {
        node.Modulate = color;
    }
}
