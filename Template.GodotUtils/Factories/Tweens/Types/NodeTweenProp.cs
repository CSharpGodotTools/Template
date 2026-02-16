using Godot;
using static Godot.Tween;

namespace GodotUtils;

/// <summary>
/// Fluent API for animating a node property.
/// </summary>
public sealed class NodeTweenProp : BaseTween<NodeTweenProp>
{
    protected override NodeTweenProp Self => this;

    private readonly string _property;

    /// <summary>
    /// Creates a tween for the specified node property.
    /// </summary>
    internal NodeTweenProp(Node node, string property) : base(node)
    {
        _property = property;
    }

    /// <summary>
    /// Tweens the property to the target value over the given duration.
    /// </summary>
    public NodeTweenProp PropertyTo(Variant finalValue, double duration)
    {
        _tweener = _tween
            .TweenProperty(_node, _property, finalValue, duration)
            .SetTrans(TransitionType.Sine);

        return this;
    }
}
