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
    internal ShaderTweenProp(Node node, string shaderParam) : base(node)
    {
        if (node is not CanvasItem canvasItem)
        {
            throw new Exception("ShaderTweenProp only supports CanvasItem-derived nodes (Node2D, Control).");
        }

        if (canvasItem.Material is not ShaderMaterial shaderMaterial)
        {
            throw new Exception("Animating shader material has not been set. Ensure the node has a ShaderMaterial assigned.");
        }

        _shaderMaterial = shaderMaterial;
        _shaderParam = shaderParam;
    }

    /// <summary>
    /// Tweens the shader parameter to the target value over the given duration.
    /// </summary>
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
