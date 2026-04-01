using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for animation trees.
/// </summary>
public static class AnimationTreeExtensions
{
    /// <summary>
    /// Sets a condition briefly and auto-resets after 0.1 seconds.
    /// </summary>
    /// <param name="tree">Animation tree to update.</param>
    /// <param name="path">Condition parameter path segment.</param>
    /// <param name="value">Condition value to set briefly.</param>
    public static void SetCondition(this AnimationTree tree, StringName path, bool value)
    {
        tree.SetParam($"conditions/{path}", value);

        new NodeTween(tree)
            .Delay(0.1)
            .Then(() => tree.SetParam($"conditions/{path}", !value));
    }

    /// <summary>
    /// Sets the blend position of a BlendSpace1D by name.
    /// </summary>
    /// <param name="tree">Animation tree to update.</param>
    /// <param name="name">Blend space parameter name.</param>
    /// <param name="value">Blend position value.</param>
    public static void SetBlendSpace1DPosition(this AnimationTree tree, StringName name, float value)
    {
        tree.SetParam($"{name}/blend_position", value);
    }

    /// <summary>
    /// Sets a parameter value on the animation tree.
    /// </summary>
    /// <param name="tree">Animation tree to update.</param>
    /// <param name="path">Parameter path segment under <c>parameters/</c>.</param>
    /// <param name="value">Value to assign.</param>
    public static void SetParam(this AnimationTree tree, StringName path, Variant value)
    {
        tree.Set($"parameters/{path}", value);
    }

    /// <summary>
    /// Gets a parameter value from the animation tree.
    /// </summary>
    /// <param name="tree">Animation tree to read.</param>
    /// <param name="path">Parameter path segment under <c>parameters/</c>.</param>
    /// <returns>Resolved parameter value.</returns>
    public static Variant GetParam(this AnimationTree tree, StringName path)
    {
        return tree.Get($"parameters/{path}");
    }

    /// <summary>
    /// Gets a condition value from the animation tree.
    /// </summary>
    /// <param name="tree">Animation tree to read.</param>
    /// <param name="path">Condition path segment under <c>conditions/</c>.</param>
    /// <returns>Resolved condition value.</returns>
    public static bool GetCondition(this AnimationTree tree, StringName path)
    {
        return (bool)tree.GetParam($"conditions/{path}");
    }

    /// <summary>
    /// Gets the state machine playback controller.
    /// </summary>
    /// <param name="tree">Animation tree to read.</param>
    /// <returns>State machine playback controller.</returns>
    public static AnimationNodeStateMachinePlayback GetStateMachine(this AnimationTree tree)
    {
        return tree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
    }
}
