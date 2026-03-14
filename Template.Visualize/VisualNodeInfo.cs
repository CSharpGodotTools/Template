#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class VisualNodeInfo(IReadOnlyList<Action> actions, Node anchorNode, Node visualRoot, Action updatePosition)
{
    public IReadOnlyList<Action> Actions { get; } = actions ?? throw new ArgumentNullException(nameof(actions));
    public Node AnchorNode { get; } = anchorNode ?? throw new ArgumentNullException(nameof(anchorNode));
    public Node VisualRoot { get; } = visualRoot ?? throw new ArgumentNullException(nameof(visualRoot));
    public Action UpdatePosition { get; } = updatePosition ?? throw new ArgumentNullException(nameof(updatePosition));
}
#endif
