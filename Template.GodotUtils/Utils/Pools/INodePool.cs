#nullable enable
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Common interface for pool operations across all pool types.
/// Abstracts pool lifecycle and access patterns.
/// </summary>
/// <typeparam name="TNode">Node type managed by the pool.</typeparam>
public interface INodePool<TNode>
{
    /// <summary>
    /// Nodes currently in use by the pool.
    /// </summary>
    /// <value>Enumerable view of nodes currently acquired from the pool.</value>
    IEnumerable<TNode> ActiveNodes { get; }

    /// <summary>
    /// Acquires a node from the pool, creating one if necessary.
    /// </summary>
    /// <returns>Active node instance ready for use.</returns>
    TNode Acquire();

    /// <summary>
    /// Releases a node back to the pool for reuse.
    /// </summary>
    /// <param name="node">Node to return to the pool.</param>
    void Release(TNode node);

    /// <summary>
    /// Immediately frees all nodes in the pool (active and inactive).
    /// </summary>
    void QueueFreeAll();
}
