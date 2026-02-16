using Godot;
using System;

namespace GodotUtils;

/// <summary>
/// Factory helpers for tween builders.
/// </summary>
public static class Tweens
{
    /// <summary>
    /// Creates a <see cref="NodeTween"/> bound to <paramref name="node"/>.
    /// </summary>
    /// <returns>The <see cref="NodeTween"/> for chain building.</returns>
    public static NodeTween Animate(Node node)
    {
        return new NodeTween(node);
    }

    /// <summary>
    /// Creates a <see cref="NodeTweenProp"/> bound to <paramref name="node"/> ready to animate on <paramref name="property"/>.
    /// </summary>
    /// <returns>The <see cref="NodeTweenProp"/> for chain building.</returns>
    public static NodeTweenProp Animate(Node node, string property)
    {
        return new NodeTweenProp(node, property);
    }

    /// <summary>
    /// Creates a <see cref="NodeTween2D"/> bound to <paramref name="node"/>.
    /// </summary>
    /// <returns>The <see cref="NodeTween2D"/> for chain building.</returns>
    public static NodeTween2D Animate(Node2D node)
    {
        return new NodeTween2D(node);
    }

    /// <summary>
    /// Creates a <see cref="NodeTweenControl"/> bound to <paramref name="control"/>.
    /// </summary>
    /// <returns>The <see cref="NodeTweenControl"/> for chain building.</returns>
    public static NodeTweenControl Animate(Control control)
    {
        return new NodeTweenControl(control);
    }

    /// <summary>
    /// Creates a <see cref="ShaderTween"/> bound to <paramref name="canvasItem"/>.
    /// </summary>
    /// <returns>The <see cref="ShaderTween"/> for chain building.</returns>
    public static ShaderTween AnimateShader(CanvasItem canvasItem)
    {
        return new ShaderTween(canvasItem);
    }

    /// <summary>
    /// Creates a <see cref="ShaderTween"/> bound to <paramref name="canvasItem"/> ready to animate <paramref name="property"/>.
    /// </summary>
    /// <returns>The <see cref="ShaderTween"/> for chain building.</returns>
    public static ShaderTweenProp AnimateShader(CanvasItem canvasItem, string property)
    {
        return new ShaderTweenProp(canvasItem, property);
    }

    /// <summary>
    /// Creates a tween bound to <paramref name="node"/> ready to animate on the <paramref name="property"/>.
    /// </summary>
    /// <returns>The tween for chain building.</returns>
    public static NodeTweenProp Animate(Node2D node, string property)
    {
        return new NodeTweenProp(node, property);
    }

    /// <summary>
    /// Creates a tween bound to <paramref name="control"/> ready to animate on the <paramref name="property"/>.
    /// </summary>
    /// <returns>The tween for chain building.</returns>
    public static NodeTweenProp Animate(Control control, string property)
    {
        return new NodeTweenProp(control, property);
    }

    /// <summary>
    /// Creates a <see cref="NodeTween"/> bound to <paramref name="node"/> and runs <paramref name="callback"/> after <paramref name="seconds"/>.
    /// </summary>
    public static NodeTween Delay(Node node, double seconds, Action callback)
    {
        NodeTween tween = new(node);

        tween.Delay(seconds)
             .Then(callback);

        return tween;
    }

    /// <summary>
    /// Creates a <see cref="NodeTween2D"/> bound to <paramref name="node"/> and runs <paramref name="callback"/> after <paramref name="seconds"/>.
    /// </summary>
    public static NodeTween2D Delay(Node2D node, double seconds, Action callback)
    {
        NodeTween2D tween = new(node);

        tween.Delay(seconds)
             .Then(callback);

        return tween;
    }

    /// <summary>
    /// Creates a <see cref="NodeTweenControl"/> bound to <paramref name="control"/> and runs <paramref name="callback"/> after <paramref name="seconds"/>.
    /// </summary>
    public static NodeTweenControl Delay(Control control, double seconds, Action callback)
    {
        NodeTweenControl tween = new(control);

        tween.Delay(seconds)
             .Then(callback);

        return tween;
    }
}
