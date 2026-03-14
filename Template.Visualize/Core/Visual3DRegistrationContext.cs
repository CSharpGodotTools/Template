#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class Visual3DRegistrationContext(
    Node anchorNode,
    Node3D anchorNode3D,
    Control visualPanel,
    IReadOnlyList<Action> actions,
    ICollection<VisualNodeInfo> existingTrackers)
{
    public Node AnchorNode { get; } = anchorNode;
    public Node3D AnchorNode3D { get; } = anchorNode3D;
    public Control VisualPanel { get; } = visualPanel;
    public IReadOnlyList<Action> Actions { get; } = actions;
    public ICollection<VisualNodeInfo> ExistingTrackers { get; } = existingTrackers;
}
#endif
