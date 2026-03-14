#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal static class VisualNodeLocator
{
    public static Node? GetClosestParentOfType(Node node, params Type[] typesToCheck)
    {
        if (IsNodeOfType(node, typesToCheck))
        {
            return node;
        }

        Node? parent = node.GetParent();

        while (parent != null)
        {
            if (IsNodeOfType(parent, typesToCheck))
            {
                return parent;
            }

            parent = parent.GetParent();
        }

        return null;
    }

    public static bool TryGetGlobalPosition2D(Node? node, out Vector2 position)
    {
        if (node is Node2D node2D)
        {
            position = node2D.GlobalPosition;
            return true;
        }

        if (node is Control control)
        {
            position = control.GlobalPosition;
            return true;
        }

        position = default;
        return false;
    }

    private static bool IsNodeOfType(Node node, Type[] typesToCheck)
    {
        foreach (Type type in typesToCheck)
        {
            if (type.IsInstanceOfType(node))
            {
                return true;
            }
        }

        return false;
    }
}
#endif
