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
    /// <param name="node">Node to animate.</param>
    /// <param name="property">Property path to animate.</param>
    internal NodeTweenProp(Node node, string property) : base(node)
    {
        _property = property;
    }

    /// <summary>
    /// Tweens the property to the target value over the given duration.
    /// </summary>
    /// <param name="finalValue">Target value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenProp PropertyTo(Variant finalValue, double duration)
    {
        _tweener = _tween
            .TweenProperty(_node, _property, finalValue, duration)
            .SetTrans(TransitionType.Sine);

        return this;
    }
}
