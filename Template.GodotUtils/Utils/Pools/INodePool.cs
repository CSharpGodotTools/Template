#nullable enable
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Common interface for pool operations across all pool types.
/// Abstracts pool lifecycle and access patterns.
/// </summary>
public interface INodePool<TNode>
{
    /// <summary>
    /// Nodes currently in use by the pool.
    /// </summary>
    IEnumerable<TNode> ActiveNodes { get; }

    /// <summary>
    /// Acquires a node from the pool, creating one if necessary.
    /// </summary>
    TNode Acquire();

    /// <summary>
    /// Releases a node back to the pool for reuse.
    /// </summary>
    void Release(TNode node);

    /// <summary>
    /// Immediately frees all nodes in the pool (active and inactive).
    /// </summary>
    void QueueFreeAll();
}
