#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Stores visualization state and callbacks for a tracked scene node.
/// </summary>
/// <param name="actions">Per-frame actions that refresh displayed values.</param>
/// <param name="anchorNode">Original node whose values are being visualized.</param>
/// <param name="visualRoot">Root node for generated visualization content.</param>
/// <param name="updatePosition">Callback that updates visualization position to follow its anchor.</param>
internal sealed class VisualNodeInfo(IReadOnlyList<Action> actions, Node anchorNode, Node visualRoot, Action updatePosition)
{
    /// <summary>
    /// Per-frame actions that refresh displayed values.
    /// </summary>
    public IReadOnlyList<Action> Actions { get; } = actions ?? throw new ArgumentNullException(nameof(actions));

    /// <summary>
    /// Original node whose data is being visualized.
    /// </summary>
    public Node AnchorNode { get; } = anchorNode ?? throw new ArgumentNullException(nameof(anchorNode));

    /// <summary>
    /// Root node of the generated visualization UI/3D content.
    /// </summary>
    public Node VisualRoot { get; } = visualRoot ?? throw new ArgumentNullException(nameof(visualRoot));

    /// <summary>
    /// Callback that repositions the visualization to follow its anchor.
    /// </summary>
    public Action UpdatePosition { get; } = updatePosition ?? throw new ArgumentNullException(nameof(updatePosition));
}
#endif
