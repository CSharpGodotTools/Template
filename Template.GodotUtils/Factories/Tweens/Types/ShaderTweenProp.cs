using Godot;
using System;
using static Godot.Tween;

namespace GodotUtils;

/// <summary>
/// Fluent API for animating a single shader parameter on a CanvasItem.
/// </summary>
public sealed class ShaderTweenProp : BaseTween<ShaderTweenProp>
{
    protected override ShaderTweenProp Self => this;

    private readonly ShaderMaterial _shaderMaterial;
    private readonly string _shaderParam;

    /// <summary>
    /// Creates a tween for the given shader parameter on a CanvasItem.
    /// </summary>
    /// <param name="node">Canvas item node to animate.</param>
    /// <param name="shaderParam">Shader parameter name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="node"/> is not a <see cref="CanvasItem"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when node does not have a <see cref="ShaderMaterial"/>.
    /// </exception>
    internal ShaderTweenProp(Node node, string shaderParam) : base(node)
    {
        // Restrict shader tweening to CanvasItem-derived nodes.
        if (node is not CanvasItem canvasItem)
        {
            throw new ArgumentException("ShaderTweenProp only supports CanvasItem-derived nodes (Node2D, Control).", nameof(node));
        }

        // Require a ShaderMaterial so shader parameters can be animated.
        if (canvasItem.Material is not ShaderMaterial shaderMaterial)
        {
            throw new InvalidOperationException("Animating shader material has not been set. Ensure the node has a ShaderMaterial assigned.");
        }

        _shaderMaterial = shaderMaterial;
        _shaderParam = shaderParam;
    }

    /// <summary>
    /// Tweens the shader parameter to the target value over the given duration.
    /// </summary>
    /// <param name="finalValue">Target parameter value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public ShaderTweenProp PropertyTo(Variant finalValue, double duration)
    {
        _tweener = _tween
            .TweenProperty(
                _shaderMaterial,
                $"shader_parameter/{_shaderParam}",
                finalValue,
                duration)
            .SetTrans(TransitionType.Sine);

        return this;
    }
}
