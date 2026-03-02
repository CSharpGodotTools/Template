#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class VisualNodeInfo(IReadOnlyList<Action> actions, Control visualControl, Node node, Vector2 offset)
{
    public IReadOnlyList<Action> Actions { get; } = actions ?? throw new ArgumentNullException(nameof(actions));
    public Control VisualControl { get; } = visualControl ?? throw new ArgumentNullException(nameof(visualControl));
    public Vector2 Offset { get; } = offset;
    public Node Node { get; } = node ?? throw new ArgumentNullException(nameof(node));
}
#endif
