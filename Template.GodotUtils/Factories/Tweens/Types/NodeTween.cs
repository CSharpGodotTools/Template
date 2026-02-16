using Godot;

namespace GodotUtils;

/// <summary>
/// Tween wrapper for a basic Node.
/// </summary>
public class NodeTween : BaseTween<NodeTween>
{
    protected override NodeTween Self => this;

    /// <summary>
    /// Creates a tween bound to the provided node.
    /// </summary>
    internal NodeTween(Node node) : base(node)
    {
    }
}
