using Godot;

namespace GodotUtils;

/// <summary>
/// Provides tweening functionality for Control properties.
/// </summary>
public class NodeTweenControl : BaseTween<NodeTweenControl>
{
    protected override NodeTweenControl Self => this;

    /// <summary>
    /// Creates a tween bound to the provided control.
    /// </summary>
    /// <param name="control">Control that receives tweened property updates.</param>
    internal NodeTweenControl(Control control) : base(control)
    {
    }

    // Position
    /// <summary>
    /// Tweens the local position.
    /// </summary>
    /// <param name="position">Target local position.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl Position(Vector2 position, double duration) => Property(Control.PropertyName.Position, position, duration);

    /// <summary>
    /// Tweens the local X position.
    /// </summary>
    /// <param name="x">Target local X value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl PositionX(double x, double duration) => Property("position:x", x, duration);

    /// <summary>
    /// Tweens the local Y position.
    /// </summary>
    /// <param name="y">Target local Y value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl PositionY(double y, double duration) => Property("position:y", y, duration);

    /// <summary>
    /// Tweens the local Z position.
    /// </summary>
    /// <param name="z">Target local Z value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl PositionZ(double z, double duration) => Property("position:z", z, duration);

    /// <summary>
    /// Tweens the global position.
    /// </summary>
    /// <param name="globalPosition">Target global position.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl GlobalPosition(Vector2 globalPosition, double duration) => Property(Control.PropertyName.GlobalPosition, globalPosition, duration);

    /// <summary>
    /// Tweens the global X position.
    /// </summary>
    /// <param name="x">Target global X value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl GlobalPositionX(double x, double duration) => Property("global_position:x", x, duration);

    /// <summary>
    /// Tweens the global Y position.
    /// </summary>
    /// <param name="y">Target global Y value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl GlobalPositionY(double y, double duration) => Property("global_position:y", y, duration);

    /// <summary>
    /// Tweens the global Z position.
    /// </summary>
    /// <param name="z">Target global Z value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl GlobalPositionZ(double z, double duration) => Property("global_position:z", z, duration);

    // Rotation
    /// <summary>
    /// Tweens the rotation.
    /// </summary>
    /// <param name="rotation">Target rotation value.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl Rotation(double rotation, double duration) => Property(Control.PropertyName.Rotation, rotation, duration);

    /// <summary>
    /// Tweens the rotation on the X axis.
    /// </summary>
    /// <param name="x">Target rotation X component.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl RotationX(double x, double duration) => Property("rotation:x", x, duration);

    /// <summary>
    /// Tweens the rotation on the Y axis.
    /// </summary>
    /// <param name="y">Target rotation Y component.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl RotationY(double y, double duration) => Property("rotation:y", y, duration);

    /// <summary>
    /// Tweens the rotation on the Z axis.
    /// </summary>
    /// <param name="z">Target rotation Z component.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl RotationZ(double z, double duration) => Property("rotation:z", z, duration);

    // Scale
    /// <summary>
    /// Tweens the scale.
    /// </summary>
    /// <param name="scale">Target local scale.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl Scale(Vector2 scale, double duration) => Property(Control.PropertyName.Scale, scale, duration);

    /// <summary>
    /// Tweens the scale on the X axis.
    /// </summary>
    /// <param name="x">Target scale X component.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl ScaleX(double x, double duration) => Property("scale:x", x, duration);

    /// <summary>
    /// Tweens the scale on the Y axis.
    /// </summary>
    /// <param name="y">Target scale Y component.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl ScaleY(double y, double duration) => Property("scale:y", y, duration);

    /// <summary>
    /// Tweens the scale on the Z axis.
    /// </summary>
    /// <param name="z">Target scale Z component.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl ScaleZ(double z, double duration) => Property("scale:z", z, duration);

    // Size
    /// <summary>
    /// Tweens the size.
    /// </summary>
    /// <param name="size">Target control size.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl Size(Vector2 size, double duration) => Property(Control.PropertyName.Size, size, duration);

    /// <summary>
    /// Tweens the custom minimum size.
    /// </summary>
    /// <param name="customMinimumSize">Target custom minimum size.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl CustomMinimumSize(Vector2 customMinimumSize, double duration) => Property(Control.PropertyName.CustomMinimumSize, customMinimumSize, duration);

    /// <summary>
    /// Tweens the pivot offset.
    /// </summary>
    /// <param name="pivotOffset">Target pivot offset.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl PivotOffset(Vector2 pivotOffset, double duration) => Property(Control.PropertyName.PivotOffset, pivotOffset, duration);

    /// <summary>
    /// Tweens the size flags stretch ratio.
    /// </summary>
    /// <param name="stretchRatio">Target size-flags stretch ratio.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl SizeFlagsStretchRatio(float stretchRatio, double duration) => Property(Control.PropertyName.SizeFlagsStretchRatio, stretchRatio, duration);

    // Color
    /// <summary>
    /// Tweens the self modulate color.
    /// </summary>
    /// <param name="color">Target self-modulate color.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl Color(Color color, double duration) => Property(CanvasItem.PropertyName.SelfModulate, color, duration);

    /// <summary>
    /// Tweens the modulate color (including children).
    /// </summary>
    /// <param name="color">Target modulate color.</param>
    /// <param name="duration">Tween duration in seconds.</param>
    /// <returns>Current tween builder for chaining.</returns>
    public NodeTweenControl ColorRecursive(Color color, double duration) => Property(CanvasItem.PropertyName.Modulate, color, duration);
}
