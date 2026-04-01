using Godot;
using System;
using static Godot.Tween;

namespace GodotUtils;

/// <summary>
/// Fluent API for animating shader parameters on a CanvasItem.
/// </summary>
public class ShaderTween : BaseTween<ShaderTween>
{
    protected override ShaderTween Self => this;

    private readonly ShaderMaterial _shaderMaterial;

    /// <summary>
    /// Creates a shader tween bound to the provided canvas item.
    /// </summary>
    /// <param name="canvasItem">Canvas item with assigned shader material.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="canvasItem"/> does not have a <see cref="ShaderMaterial"/>.
    /// </exception>
    internal ShaderTween(CanvasItem canvasItem) : base(canvasItem)
    {
        // Require a ShaderMaterial so shader parameters can be animated.
        if (canvasItem.Material is not ShaderMaterial shaderMaterial)
        {
            throw new InvalidOperationException("Animating shader material has not been set. Ensure the node has a ShaderMaterial assigned.");
        }

        _shaderMaterial = shaderMaterial;
    }

    /// <summary>
    /// Tweens a shader parameter to the target value over the given duration.
    /// </summary>
    /// <param name="shaderParam">Shader parameter name.</param>
    /// <param name="finalValue">Target parameter value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public override ShaderTween Property(string shaderParam, Variant finalValue, double duration)
    {
        _tweener = _tween
            .TweenProperty(_shaderMaterial, $"shader_parameter/{shaderParam}", finalValue, duration)
            .SetTrans(TransitionType.Sine);

        return this;
    }
}
