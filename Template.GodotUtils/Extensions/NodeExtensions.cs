using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

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
    /// <param name="node">Node used to access the current scene tree.</param>
    /// <param name="autoload">Autoload node name registered in project settings.</param>
    /// <returns>Resolved autoload node cast to <typeparamref name="T"/>.</returns>
    public static T GetAutoload<T>(this Node node, string autoload) where T : Node
    {
        return node.GetNode<T>($"/root/{autoload}");
    }

    /// <summary>
    /// Adds a child to the current scene using a deferred call.
    /// </summary>
    /// <param name="node">Node used to resolve the current scene.</param>
    /// <param name="child">Child node to add to the current scene.</param>
    public static void AddToCurrentSceneDeferred(this Node node, Node child)
    {
        GetCurrentScene(node).CallDeferred(Node.MethodName.AddChild, child);
    }

    /// <summary>
    /// Adds a child to the current scene immediately.
    /// </summary>
    /// <param name="node">Node used to resolve the current scene.</param>
    /// <param name="child">Child node to add to the current scene.</param>
    public static void AddToCurrentScene(this Node node, Node child)
    {
        GetCurrentScene(node).AddChild(child);
    }

    /// <summary>
    /// Gets a node at <paramref name="path"/> from the current scene.
    /// </summary>
    /// <param name="node">Node used to resolve the current scene.</param>
    /// <param name="path">Relative node path inside the current scene.</param>
    /// <returns>Node found at the requested path.</returns>
    public static Node GetNodeInCurrentScene(this Node node, string path)
    {
        return GetCurrentScene(node).GetNode(path);
    }

    /// <summary>
    /// Gets the current scene from the tree.
    /// </summary>
    /// <param name="node">Node used to access the scene tree.</param>
    /// <returns>Current scene root node.</returns>
    public static Node GetCurrentScene(this Node node)
    {
        return node.GetTree().CurrentScene;
    }

    /// <summary>
    /// Gets a node of type <typeparamref name="T"/> from the current scene by path.
    /// </summary>
    /// <typeparam name="T">Expected node type.</typeparam>
    /// <param name="node">Node used to access the scene tree.</param>
    /// <param name="path">Relative node path inside the current scene.</param>
    /// <returns>Matching node when found; otherwise <see langword="null"/>.</returns>
    public static T? GetSceneNode<T>(this Node node, string path) where T : Node
    {
        return node.GetTree().CurrentScene.GetNode<T>(path);
    }

    /// <summary>
    /// Gets a node of type <typeparamref name="T"/> from the current scene.
    /// </summary>
    /// <typeparam name="T">Expected node type.</typeparam>
    /// <param name="node">Node used to access the scene tree.</param>
    /// <returns>First direct child of the current scene matching <typeparamref name="T"/>.</returns>
    public static T? GetSceneNode<T>(this Node node) where T : Node
    {
        return node.GetTree().CurrentScene.GetNode<T>(recursive: false);
    }

    /// <summary>
    /// Recursively searches for all nodes of a specific type.
    /// </summary>
    /// <param name="node">Root node for the search.</param>
    /// <param name="type">Exact runtime type to include in results.</param>
    /// <returns>All matching nodes discovered under the root.</returns>
    public static List<Node> GetNodes(this Node node, Type type)
    {
        List<Node> nodes = [];
        RecursiveTypeMatchSearch(node, type, nodes);
        return nodes;
    }

    /// <summary>
    /// Traverses the node tree depth-first and collects nodes whose runtime type equals the target type.
    /// </summary>
    /// <param name="node">Current node to evaluate.</param>
    /// <param name="type">Exact runtime type to match.</param>
    /// <param name="nodes">Accumulator for matched nodes.</param>
    private static void RecursiveTypeMatchSearch(Node node, Type type, List<Node> nodes)
    {
        // Collect node when its runtime type matches target exactly.
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
    /// <typeparam name="T">Target node type to locate.</typeparam>
    /// <param name="node">Root node whose children are searched.</param>
    /// <param name="foundNode">Resolved node when one is found.</param>
    /// <param name="recursive">Whether descendants are included in the search.</param>
    /// <returns><see langword="true"/> when a matching node is found.</returns>
    public static bool TryGetNode<T>(this Node node, [NotNullWhen(true)] out T? foundNode, bool recursive = true) where T : Node
    {
        foundNode = FindNode<T>(node.GetChildren(), recursive);
        return foundNode != null;
    }

    /// <summary>
    /// Returns true when a child node of type <typeparamref name="T"/> exists.
    /// </summary>
    /// <typeparam name="T">Target node type to locate.</typeparam>
    /// <param name="node">Root node whose children are searched.</param>
    /// <param name="recursive">Whether descendants are included in the search.</param>
    /// <returns><see langword="true"/> when a matching node exists.</returns>
    public static bool HasNode<T>(this Node node, bool recursive = true) where T : Node
    {
        return FindNode<T>(node.GetChildren(), recursive) != null;
    }

    /// <summary>
    /// Finds a child node of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target node type to locate.</typeparam>
    /// <param name="node">Root node whose children are searched.</param>
    /// <param name="recursive">Whether descendants are included in the search.</param>
    /// <returns>First matching node when found; otherwise <see langword="null"/>.</returns>
    [Obsolete("Use GetNode<T>() instead.")]
    public static T? GetComponent<T>(this Node node, bool recursive = true) where T : Node
    {
        return GetNode<T>(node, recursive);
    }

    /// <summary>
    /// Finds a child node of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target node type to locate.</typeparam>
    /// <param name="node">Root node whose children are searched.</param>
    /// <param name="recursive">Whether descendants are included in the search.</param>
    /// <returns>First matching node when found; otherwise <see langword="null"/>.</returns>
    public static T? GetNode<T>(this Node node, bool recursive = true) where T : Node
    {
        return FindNode<T>(node.GetChildren(), recursive);
    }

    /// <summary>
    /// Finds the first child node assignable to <typeparamref name="T"/> in the provided collection.
    /// </summary>
    /// <typeparam name="T">Target node type.</typeparam>
    /// <param name="children">Children to inspect.</param>
    /// <param name="recursive">Whether descendants should be searched recursively.</param>
    /// <returns>First matching node; otherwise <see langword="null"/>.</returns>
    private static T? FindNode<T>(Godot.Collections.Array<Node> children, bool recursive = true) where T : Node
    {
        foreach (Node child in children)
        {
            // Return first direct match.
            if (child is T type)
                return type;

            // Descend into child hierarchy when recursive search is enabled.
            if (recursive)
            {
                T? val = FindNode<T>(child.GetChildren());

                // Bubble up first recursive match.
                if (val is not null)
                    return val;
            }
        }

        return null;
    }

    /// <summary>
    /// Asynchronously waits for one process frame.
    /// </summary>
    /// <param name="parent">Node whose scene tree emits the process-frame signal.</param>
    /// <returns>Task that completes on the next process frame.</returns>
    public static async Task WaitOneFrame(this Node parent)
    {
        await parent.ToSignal(
            source: parent.GetTree(),
            signal: SceneTree.SignalName.ProcessFrame);
    }

    /// <summary>
    /// Adds a child using a deferred call.
    /// </summary>
    /// <param name="node">Parent node that receives the child.</param>
    /// <param name="child">Child node to add.</param>
    public static void AddChildDeferred(this Node node, Node child)
    {
        node.CallDeferred(Node.MethodName.AddChild, child);
    }

    /// <summary>
    /// Recursively retrieves all nodes of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target node type to collect.</typeparam>
    /// <param name="node">Root node whose children are scanned.</param>
    /// <param name="recursive">Whether descendants are included in the traversal.</param>
    /// <returns>List of matching child nodes.</returns>
    public static List<T> GetChildren<T>(this Node node, bool recursive = true) where T : Node
    {
        List<T> children = [];
        FindChildrenOfType(node, children, recursive);
        return children;
    }

    /// <summary>
    /// Recursively appends child nodes assignable to <typeparamref name="T"/> to the destination list.
    /// </summary>
    /// <typeparam name="T">Target node type.</typeparam>
    /// <param name="node">Root node whose children are scanned.</param>
    /// <param name="children">Destination list for all matched child nodes.</param>
    /// <param name="recursive">Whether descendants should be traversed recursively.</param>
    private static void FindChildrenOfType<T>(Node node, List<T> children, bool recursive) where T : Node
    {
        foreach (Node child in node.GetChildren())
        {
            // Collect all children assignable to target type.
            if (child is T typedChild)
                children.Add(typedChild);

            // Continue traversal when recursive mode is enabled.
            if (recursive)
            {
                FindChildrenOfType(child, children, recursive);
            }
        }
    }

    /// <summary>
    /// Queue frees all children attached to this node.
    /// </summary>
    /// <param name="parentNode">Parent node whose children will be queued for deletion.</param>
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
    /// <param name="node">Node to detach from all assigned groups.</param>
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
    /// <param name="node">Current traversal node.</param>
    /// <param name="code">Callback invoked for each visited node.</param>
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
