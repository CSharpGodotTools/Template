#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Input data required to register a visualization panel for a 2D/canvas anchor node.
/// </summary>
/// <param name="anchorNode">Anchor node whose lifecycle owns the visualization.</param>
/// <param name="positionalNode">Node providing 2D position updates for placement.</param>
/// <param name="visualPanel">Visual panel control to register.</param>
/// <param name="actions">Per-frame actions associated with the visualization.</param>
/// <param name="existingTrackers">Existing trackers used for overlap/offset calculations.</param>
internal sealed class Visual2DRegistrationContext(
    Node anchorNode,
    Node positionalNode,
    Control visualPanel,
    IReadOnlyList<Action> actions,
    ICollection<VisualNodeInfo> existingTrackers)
{
    /// <summary>
    /// Anchor node whose lifecycle controls visualization cleanup.
    /// </summary>
    public Node AnchorNode { get; } = anchorNode;

    /// <summary>
    /// Node providing global 2D position updates for panel placement.
    /// </summary>
    public Node PositionalNode { get; } = positionalNode;

    /// <summary>
    /// Visual panel shown in a canvas layer.
    /// </summary>
    public Control VisualPanel { get; } = visualPanel;

    /// <summary>
    /// Per-frame update actions associated with this visualization.
    /// </summary>
    public IReadOnlyList<Action> Actions { get; } = actions;

    /// <summary>
    /// Existing trackers used to compute overlap offsets.
    /// </summary>
    public ICollection<VisualNodeInfo> ExistingTrackers { get; } = existingTrackers;
}
#endif
