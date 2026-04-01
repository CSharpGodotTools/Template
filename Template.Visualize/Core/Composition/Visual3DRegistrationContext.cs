#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Input data required to register a visualization panel for a 3D anchor node.
/// </summary>
/// <param name="anchorNode">Anchor node whose lifecycle owns the visualization.</param>
/// <param name="anchorNode3D">3D anchor used for world-space placement updates.</param>
/// <param name="visualPanel">Visual panel control rendered for this tracker.</param>
/// <param name="actions">Per-frame actions associated with the visualization.</param>
/// <param name="existingTrackers">Existing trackers used for overlap/offset calculations.</param>
internal sealed class Visual3DRegistrationContext(
    Node anchorNode,
    Node3D anchorNode3D,
    Control visualPanel,
    IReadOnlyList<Action> actions,
    ICollection<VisualNodeInfo> existingTrackers)
{
    /// <summary>
    /// Anchor node whose lifecycle controls visualization cleanup.
    /// </summary>
    public Node AnchorNode { get; } = anchorNode;

    /// <summary>
    /// 3D positional anchor used for world-space placement.
    /// </summary>
    public Node3D AnchorNode3D { get; } = anchorNode3D;

    /// <summary>
    /// Visual panel rendered into a sub-viewport texture.
    /// </summary>
    public Control VisualPanel { get; } = visualPanel;

    /// <summary>
    /// Per-frame update actions associated with this visualization.
    /// </summary>
    public IReadOnlyList<Action> Actions { get; } = actions;

    /// <summary>
    /// Existing trackers used to compute stacking offsets.
    /// </summary>
    public ICollection<VisualNodeInfo> ExistingTrackers { get; } = existingTrackers;
}
#endif
