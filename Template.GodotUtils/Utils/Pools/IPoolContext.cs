using Godot;

namespace GodotUtils;

/// <summary>
/// Context passed to pooled nodes for releasing themselves.
/// </summary>
public interface IPoolContext<in TNode> where TNode : CanvasItem
{
    /// <summary>
    /// Releases a node back into the pool.
    /// </summary>
    void Release(TNode node);
}
