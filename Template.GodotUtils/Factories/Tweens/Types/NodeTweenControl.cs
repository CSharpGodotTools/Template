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
    internal NodeTweenControl(Control control) : base(control)
    {
    }

    // Position
    /// <summary>
    /// Tweens the local position.
    /// </summary>
    public NodeTweenControl Position(Vector2 position, double duration) => (NodeTweenControl)Property(Control.PropertyName.Position, position, duration);

    /// <summary>
    /// Tweens the local X position.
    /// </summary>
    public NodeTweenControl PositionX(double x, double duration) => (NodeTweenControl)Property("position:x", x, duration);

    /// <summary>
    /// Tweens the local Y position.
    /// </summary>
    public NodeTweenControl PositionY(double y, double duration) => (NodeTweenControl)Property("position:y", y, duration);

    /// <summary>
    /// Tweens the local Z position.
    /// </summary>
    public NodeTweenControl PositionZ(double z, double duration) => (NodeTweenControl)Property("position:z", z, duration);

    /// <summary>
    /// Tweens the global position.
    /// </summary>
    public NodeTweenControl GlobalPosition(Vector2 globalPosition, double duration) => (NodeTweenControl)Property(Control.PropertyName.GlobalPosition, globalPosition, duration);

    /// <summary>
    /// Tweens the global X position.
    /// </summary>
    public NodeTweenControl GlobalPositionX(double x, double duration) => (NodeTweenControl)Property("global_position:x", x, duration);

    /// <summary>
    /// Tweens the global Y position.
    /// </summary>
    public NodeTweenControl GlobalPositionY(double y, double duration) => (NodeTweenControl)Property("global_position:y", y, duration);

    /// <summary>
    /// Tweens the global Z position.
    /// </summary>
    public NodeTweenControl GlobalPositionZ(double z, double duration) => (NodeTweenControl)Property("global_position:z", z, duration);

    // Rotation
    /// <summary>
    /// Tweens the rotation.
    /// </summary>
    public NodeTweenControl Rotation(double rotation, double duration) => (NodeTweenControl)Property(Control.PropertyName.Rotation, rotation, duration);

    /// <summary>
    /// Tweens the rotation on the X axis.
    /// </summary>
    public NodeTweenControl RotationX(double x, double duration) => (NodeTweenControl)Property("rotation:x", x, duration);

    /// <summary>
    /// Tweens the rotation on the Y axis.
    /// </summary>
    public NodeTweenControl RotationY(double y, double duration) => (NodeTweenControl)Property("rotation:y", y, duration);

    /// <summary>
    /// Tweens the rotation on the Z axis.
    /// </summary>
    public NodeTweenControl RotationZ(double z, double duration) => (NodeTweenControl)Property("rotation:z", z, duration);

    // Scale
    /// <summary>
    /// Tweens the scale.
    /// </summary>
    public NodeTweenControl Scale(Vector2 scale, double duration) => (NodeTweenControl)Property(Control.PropertyName.Scale, scale, duration);

    /// <summary>
    /// Tweens the scale on the X axis.
    /// </summary>
    public NodeTweenControl ScaleX(double x, double duration) => (NodeTweenControl)Property("scale:x", x, duration);

    /// <summary>
    /// Tweens the scale on the Y axis.
    /// </summary>
    public NodeTweenControl ScaleY(double y, double duration) => (NodeTweenControl)Property("scale:y", y, duration);

    /// <summary>
    /// Tweens the scale on the Z axis.
    /// </summary>
    public NodeTweenControl ScaleZ(double z, double duration) => (NodeTweenControl)Property("scale:z", z, duration);

    // Size
    /// <summary>
    /// Tweens the size.
    /// </summary>
    public NodeTweenControl Size(Vector2 size, double duration) => (NodeTweenControl)Property(Control.PropertyName.Size, size, duration);

    /// <summary>
    /// Tweens the custom minimum size.
    /// </summary>
    public NodeTweenControl CustomMinimumSize(Vector2 customMinimumSize, double duration) => (NodeTweenControl)Property(Control.PropertyName.CustomMinimumSize, customMinimumSize, duration);

    /// <summary>
    /// Tweens the pivot offset.
    /// </summary>
    public NodeTweenControl PivotOffset(Vector2 pivotOffset, double duration) => (NodeTweenControl)Property(Control.PropertyName.PivotOffset, pivotOffset, duration);

    /// <summary>
    /// Tweens the size flags stretch ratio.
    /// </summary>
    public NodeTweenControl SizeFlagsStretchRatio(float stretchRatio, double duration) => (NodeTweenControl)Property(Control.PropertyName.SizeFlagsStretchRatio, stretchRatio, duration);

    // Color
    /// <summary>
    /// Tweens the self modulate color.
    /// </summary>
    public NodeTweenControl Color(Color color, double duration) => (NodeTweenControl)Property(CanvasItem.PropertyName.SelfModulate, color, duration);

    /// <summary>
    /// Tweens the modulate color (including children).
    /// </summary>
    public NodeTweenControl ColorRecursive(Color color, double duration) => (NodeTweenControl)Property(CanvasItem.PropertyName.Modulate, color, duration);
}
