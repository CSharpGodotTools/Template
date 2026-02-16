using Godot;
using static Godot.Tween;
using System;

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
    internal ShaderTween(CanvasItem canvasItem) : base(canvasItem)
    {
        if (canvasItem.Material is not ShaderMaterial shaderMaterial)
        {
            throw new Exception("Animating shader material has not been set. Ensure the node has a ShaderMaterial assigned.");
        }

        _shaderMaterial = shaderMaterial;
    }

    /// <summary>
    /// Tweens a shader parameter to the target value over the given duration.
    /// </summary>
    public override ShaderTween Property(string shaderParam, Variant finalValue, double duration)
    {
        _tweener = _tween
            .TweenProperty(_shaderMaterial, $"shader_parameter/{shaderParam}", finalValue, duration)
            .SetTrans(TransitionType.Sine);

        return this;
    }
}
