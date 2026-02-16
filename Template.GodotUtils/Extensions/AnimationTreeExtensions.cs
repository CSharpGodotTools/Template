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
    public static void SetBlendSpace1DPosition(this AnimationTree tree, StringName name, float value)
    {
        tree.SetParam($"{name}/blend_position", value);
    }

    /// <summary>
    /// Sets a parameter value on the animation tree.
    /// </summary>
    public static void SetParam(this AnimationTree tree, StringName path, Variant value)
    {
        tree.Set($"parameters/{path}", value);
    }

    /// <summary>
    /// Gets a parameter value from the animation tree.
    /// </summary>
    public static Variant GetParam(this AnimationTree tree, StringName path)
    {
        return tree.Get($"parameters/{path}");
    }

    /// <summary>
    /// Gets a condition value from the animation tree.
    /// </summary>
    public static bool GetCondition(this AnimationTree tree, StringName path)
    {
        return (bool)tree.GetParam($"conditions/{path}");
    }

    /// <summary>
    /// Gets the state machine playback controller.
    /// </summary>
    public static AnimationNodeStateMachinePlayback GetStateMachine(this AnimationTree tree)
    {
        return tree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
    }
}
