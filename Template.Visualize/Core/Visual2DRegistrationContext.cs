#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class Visual2DRegistrationContext(
    Node anchorNode,
    Node positionalNode,
    Control visualPanel,
    IReadOnlyList<Action> actions,
    ICollection<VisualNodeInfo> existingTrackers)
{
    public Node AnchorNode { get; } = anchorNode;
    public Node PositionalNode { get; } = positionalNode;
    public Control VisualPanel { get; } = visualPanel;
    public IReadOnlyList<Action> Actions { get; } = actions;
    public ICollection<VisualNodeInfo> ExistingTrackers { get; } = existingTrackers;
}
#endif
