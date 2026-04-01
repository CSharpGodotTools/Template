using Godot;

namespace GodotUtils;

/// <summary>
/// Context passed to pooled nodes for releasing themselves.
/// </summary>
/// <typeparam name="TNode">Node type handled by the pool context.</typeparam>
public interface IPoolContext<in TNode> where TNode : CanvasItem
{
    /// <summary>
    /// Releases a node back into the pool.
    /// </summary>
    /// <param name="node">Node instance to release.</param>
    void Release(TNode node);
}
