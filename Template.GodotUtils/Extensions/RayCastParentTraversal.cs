using Godot;
using System;

namespace GodotUtils;

/// <summary>
/// Shared parent-chain traversal helper for raycast extension utilities.
/// </summary>
internal static class RayCastParentTraversal
{
    /// <summary>
    /// Iterates parent nodes from nearest to root, invoking callback for each.
    /// </summary>
    /// <param name="parent">Starting parent node.</param>
    /// <param name="callback">Action invoked for each parent node.</param>
    public static void ForEachParent(Node? parent, Action<Node> callback)
    {
        while (parent != null)
        {
            callback(parent);
            parent = parent.GetParentOrNull<Node>();
        }
    }
}
