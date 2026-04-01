namespace GodotUtils;

/// <summary>
/// Decision returned by a traversal callback.
/// </summary>
public enum TraverseDecision
{
    /// <summary>
    /// Continue traversal normally.
    /// </summary>
    Continue,

    /// <summary>
    /// Skip traversing children of the current directory entry.
    /// </summary>
    SkipChildren,

    /// <summary>
    /// Stop traversal immediately.
    /// </summary>
    Stop
}
