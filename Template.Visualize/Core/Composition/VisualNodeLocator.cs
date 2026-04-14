#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Resolves positional nodes and coordinates used to anchor visualization panels.
/// </summary>
internal static class VisualNodeLocator
{
    /// <summary>
    /// Returns the closest node in the parent chain that matches any requested type.
    /// </summary>
    /// <param name="node">Starting node.</param>
    /// <param name="typesToCheck">Candidate types considered a match.</param>
    /// <returns>Closest matching node or <see langword="null"/> when no match exists.</returns>
    public static Node? GetClosestParentOfType(Node node, params Type[] typesToCheck)
    {
        // Include the current node in the match check before walking up the hierarchy.
        if (IsNodeOfType(node, typesToCheck))
            return node;

        Node? parent = node.GetParent();

        // Walk parent chain until a matching type is found.
        while (parent != null)
        {
            // Return immediately on first matching ancestor.
            if (IsNodeOfType(parent, typesToCheck))
                return parent;

            parent = parent.GetParent();
        }

        return null;
    }

    /// <summary>
    /// Attempts to resolve a global 2D position from supported node types.
    /// </summary>
    /// <param name="node">Node to inspect.</param>
    /// <param name="position">Resolved global position when supported.</param>
    /// <returns><see langword="true"/> when position could be resolved from node type.</returns>
    public static bool TryGetGlobalPosition2D(Node? node, out Vector2 position)
    {
        // Node2D provides a direct global position in 2D space.
        if (node is Node2D node2D)
        {
            position = node2D.GlobalPosition;
            return true;
        }

        // Control nodes also expose global position in canvas space.
        if (node is Control control)
        {
            position = control.GlobalPosition;
            return true;
        }

        position = default;
        return false;
    }

    /// <summary>
    /// Determines whether a node is assignable to any of the requested types.
    /// </summary>
    /// <param name="node">Node instance to test.</param>
    /// <param name="typesToCheck">Types considered a match.</param>
    /// <returns><see langword="true"/> when node matches at least one type.</returns>
    private static bool IsNodeOfType(Node node, Type[] typesToCheck)
    {
        foreach (Type type in typesToCheck)
        {
            // Accept the first assignable type match.
            if (type.IsInstanceOfType(node))
                return true;
        }

        return false;
    }
}
#endif
