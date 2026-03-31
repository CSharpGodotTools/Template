using Godot;
using System;

namespace GodotUtils;

internal static class RayCastParentTraversal
{
    public static void ForEachParent(Node? parent, Action<Node> callback)
    {
        while (parent != null)
        {
            callback(parent);
            parent = parent.GetParentOrNull<Node>();
        }
    }
}
