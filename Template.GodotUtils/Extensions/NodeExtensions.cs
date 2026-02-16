using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace GodotUtils;

/// <summary>
/// Extension helpers for nodes.
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// Retrieves an autoload from the scene tree using the given name.
    /// <code>Global global = someNode.GetAutoload&lt;Global&gt;("Global");</code>
    /// </summary>
    /// <typeparam name="T">Type of the autoload node, must inherit from Node.</typeparam>
    public static T GetAutoload<T>(this Node node, string autoload) where T : Node
    {
        return node.GetNode<T>($"/root/{autoload}");
    }

    /// <summary>
    /// Adds a child to the current scene using a deferred call.
    /// </summary>
    public static void AddToCurrentSceneDeferred(this Node node, Node child)
    {
        GetCurrentScene(node).CallDeferred(Node.MethodName.AddChild, child);
    }

    /// <summary>
    /// Adds a child to the current scene immediately.
    /// </summary>
    public static void AddToCurrentScene(this Node node, Node child)
    {
        GetCurrentScene(node).AddChild(child);
    }

    /// <summary>
    /// Gets a node at <paramref name="path"/> from the current scene.
    /// </summary>
    public static Node GetNodeInCurrentScene(this Node node, string path)
    {
        return GetCurrentScene(node).GetNode(path);
    }

    /// <summary>
    /// Gets the current scene from the tree.
    /// </summary>
    public static Node GetCurrentScene(this Node node)
    {
        return node.GetTree().CurrentScene;
    }

    /// <summary>
    /// Gets a node of type <typeparamref name="T"/> from the current scene by path.
    /// </summary>
    public static T GetSceneNode<T>(this Node node, string path) where T : Node
    {
        return node.GetTree().CurrentScene.GetNode<T>(path);
    }

    /// <summary>
    /// Gets a node of type <typeparamref name="T"/> from the current scene.
    /// </summary>
    public static T GetSceneNode<T>(this Node node) where T : Node
    {
        return node.GetTree().CurrentScene.GetNode<T>(recursive: false);
    }

    /// <summary>
    /// Recursively searches for all nodes of a specific type.
    /// </summary>
    public static List<Node> GetNodes(this Node node, Type type)
    {
        List<Node> nodes = [];
        RecursiveTypeMatchSearch(node, type, nodes);
        return nodes;
    }

    private static void RecursiveTypeMatchSearch(Node node, Type type, List<Node> nodes)
    {
        if (node.GetType() == type)
            nodes.Add(node);

        foreach (Node child in node.GetChildren())
        {
            RecursiveTypeMatchSearch(child, type, nodes);
        }
    }

    /// <summary>
    /// Attempts to find a child node of type <typeparamref name="T"/>.
    /// </summary>
    public static bool TryGetNode<T>(this Node node, out T foundNode, bool recursive = true) where T : Node
    {
        foundNode = FindNode<T>(node.GetChildren(), recursive);
        return foundNode != null;
    }

    /// <summary>
    /// Returns true when a child node of type <typeparamref name="T"/> exists.
    /// </summary>
    public static bool HasNode<T>(this Node node, bool recursive = true) where T : Node
    {
        return FindNode<T>(node.GetChildren(), recursive) != null;
    }

    /// <summary>
    /// Finds a child node of type <typeparamref name="T"/>.
    /// </summary>
    public static T GetComponent<T>(this Node node, bool recursive = true) where T : Node
    {
        return GetNode<T>(node, recursive);
    }

    /// <summary>
    /// Finds a child node of type <typeparamref name="T"/>.
    /// </summary>
    public static T GetNode<T>(this Node node, bool recursive = true) where T : Node
    {
        return FindNode<T>(node.GetChildren(), recursive);
    }

    private static T FindNode<T>(Godot.Collections.Array<Node> children, bool recursive = true) where T : Node
    {
        foreach (Node child in children)
        {
            if (child is T type)
                return type;

            if (recursive)
            {
                T val = FindNode<T>(child.GetChildren());

                if (val is not null)
                    return val;
            }
        }

        return null;
    }

    /// <summary>
    /// Asynchronously waits for one process frame.
    /// </summary>
    public static async Task WaitOneFrame(this Node parent)
    {
        await parent.ToSignal(
            source: parent.GetTree(),
            signal: SceneTree.SignalName.ProcessFrame);
    }

    /// <summary>
    /// Adds a child using a deferred call.
    /// </summary>
    public static void AddChildDeferred(this Node node, Node child)
    {
        node.CallDeferred(Node.MethodName.AddChild, child);
    }

    /// <summary>
    /// Recursively retrieves all nodes of type <typeparamref name="T"/>.
    /// </summary>
    public static List<T> GetChildren<T>(this Node node, bool recursive = true) where T : Node
    {
        List<T> children = [];
        FindChildrenOfType(node, children, recursive);
        return children;
    }

    private static void FindChildrenOfType<T>(Node node, List<T> children, bool recursive) where T : Node
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T typedChild)
                children.Add(typedChild);

            if (recursive)
            {
                FindChildrenOfType(child, children, recursive);
            }
        }
    }

    /// <summary>
    /// Queue frees all children attached to this node.
    /// </summary>
    public static void QueueFreeChildren(this Node parentNode)
    {
        foreach (Node node in parentNode.GetChildren())
        {
            node.QueueFree();
        }
    }

    /// <summary>
    /// Removes all groups this node is attached to.
    /// </summary>
    public static void RemoveAllGroups(this Node node)
    {
        Godot.Collections.Array<StringName> groups = node.GetGroups();

        for (int i = 0; i < groups.Count; i++)
        {
            node.RemoveFromGroup(groups[i]);
        }
    }

    /// <summary>
    /// Recursively traverses the tree and executes <paramref name="code"/> for each node.
    /// </summary>
    public static void TraverseNodes(this Node node, Action<Node> code)
    {
        // Execute the action on the current node
        code(node);

        // Recurse into children
        foreach (Node child in node.GetChildren())
        {
            TraverseNodes(child, code);
        }
    }
}
