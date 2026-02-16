using Godot;

namespace GodotUtils;

/// <summary>
/// Provides tweening functionality for Node2D properties.
/// </summary>
public class NodeTween2D : BaseTween<NodeTween2D>
{
    protected override NodeTween2D Self => this;

    /// <summary>
    /// Creates a tween bound to the provided Node2D.
    /// </summary>
    internal NodeTween2D(Node2D node) : base(node)
    {
    }

    // Position
    /// <summary>
    /// Tweens the local position.
    /// </summary>
    public NodeTween2D Position(Vector2 position, double duration) => (NodeTween2D)Property(Node2D.PropertyName.Position, position, duration);

    /// <summary>
    /// Tweens the local X position.
    /// </summary>
    public NodeTween2D PositionX(double x, double duration) => (NodeTween2D)Property("position:x", x, duration);

    /// <summary>
    /// Tweens the local Y position.
    /// </summary>
    public NodeTween2D PositionY(double y, double duration) => (NodeTween2D)Property("position:y", y, duration);

    /// <summary>
    /// Tweens the local Z position.
    /// </summary>
    public NodeTween2D PositionZ(double z, double duration) => (NodeTween2D)Property("position:z", z, duration);

    /// <summary>
    /// Tweens the global position.
    /// </summary>
    public NodeTween2D GlobalPosition(Vector2 globalPosition, double duration) => (NodeTween2D)Property(Node2D.PropertyName.GlobalPosition, globalPosition, duration);

    /// <summary>
    /// Tweens the global X position.
    /// </summary>
    public NodeTween2D GlobalPositionX(double x, double duration) => (NodeTween2D)Property("global_position:x", x, duration);

    /// <summary>
    /// Tweens the global Y position.
    /// </summary>
    public NodeTween2D GlobalPositionY(double y, double duration) => (NodeTween2D)Property("global_position:y", y, duration);

    /// <summary>
    /// Tweens the global Z position.
    /// </summary>
    public NodeTween2D GlobalPositionZ(double z, double duration) => (NodeTween2D)Property("global_position:z", z, duration);

    // Rotation
    /// <summary>
    /// Tweens the rotation.
    /// </summary>
    public NodeTween2D Rotation(double rotation, double duration) => (NodeTween2D)Property(Node2D.PropertyName.Rotation, rotation, duration);

    /// <summary>
    /// Tweens the rotation on the X axis.
    /// </summary>
    public NodeTween2D RotationX(double x, double duration) => (NodeTween2D)Property("rotation:x", x, duration);

    /// <summary>
    /// Tweens the rotation on the Y axis.
    /// </summary>
    public NodeTween2D RotationY(double y, double duration) => (NodeTween2D)Property("rotation:y", y, duration);

    /// <summary>
    /// Tweens the rotation on the Z axis.
    /// </summary>
    public NodeTween2D RotationZ(double z, double duration) => (NodeTween2D)Property("rotation:z", z, duration);

    /// <summary>
    /// Tweens the global rotation.
    /// </summary>
    public NodeTween2D GlobalRotation(double rotation, double duration) => (NodeTween2D)Property(Node2D.PropertyName.GlobalRotation, rotation, duration);

    /// <summary>
    /// Tweens the global rotation on the X axis.
    /// </summary>
    public NodeTween2D GlobalRotationX(double x, double duration) => (NodeTween2D)Property("global_rotation:x", x, duration);

    /// <summary>
    /// Tweens the global rotation on the Y axis.
    /// </summary>
    public NodeTween2D GlobalRotationY(double y, double duration) => (NodeTween2D)Property("global_rotation:y", y, duration);

    /// <summary>
    /// Tweens the global rotation on the Z axis.
    /// </summary>
    public NodeTween2D GlobalRotationZ(double z, double duration) => (NodeTween2D)Property("global_rotation:z", z, duration);

    // Scale
    /// <summary>
    /// Tweens the scale.
    /// </summary>
    public NodeTween2D Scale(Vector2 scale, double duration) => (NodeTween2D)Property(Node2D.PropertyName.Scale, scale, duration);

    /// <summary>
    /// Tweens the scale on the X axis.
    /// </summary>
    public NodeTween2D ScaleX(double x, double duration) => (NodeTween2D)Property("scale:x", x, duration);

    /// <summary>
    /// Tweens the scale on the Y axis.
    /// </summary>
    public NodeTween2D ScaleY(double y, double duration) => (NodeTween2D)Property("scale:y", y, duration);

    /// <summary>
    /// Tweens the scale on the Z axis.
    /// </summary>
    public NodeTween2D ScaleZ(double z, double duration) => (NodeTween2D)Property("scale:z", z, duration);

    /// <summary>
    /// Tweens the global scale.
    /// </summary>
    public NodeTween2D GlobalScale(Vector2 globalScale, double duration) => (NodeTween2D)Property(Node2D.PropertyName.GlobalScale, globalScale, duration);

    /// <summary>
    /// Tweens the global scale on the X axis.
    /// </summary>
    public NodeTween2D GlobalScaleX(double x, double duration) => (NodeTween2D)Property("global_scale:x", x, duration);

    /// <summary>
    /// Tweens the global scale on the Y axis.
    /// </summary>
    public NodeTween2D GlobalScaleY(double y, double duration) => (NodeTween2D)Property("global_scale:y", y, duration);

    /// <summary>
    /// Tweens the global scale on the Z axis.
    /// </summary>
    public NodeTween2D GlobalScaleZ(double z, double duration) => (NodeTween2D)Property("global_scale:z", z, duration);

    // Color
    /// <summary>
    /// Tweens the self modulate color.
    /// </summary>
    public NodeTween2D Color(Color color, double duration) => (NodeTween2D)Property(CanvasItem.PropertyName.SelfModulate, color, duration);

    /// <summary>
    /// Tweens the modulate color (including children).
    /// </summary>
    public NodeTween2D ColorRecursive(Color color, double duration) => (NodeTween2D)Property(CanvasItem.PropertyName.Modulate, color, duration);

    // Skew
    /// <summary>
    /// Tweens the skew.
    /// </summary>
    public NodeTween2D Skew(float skew, double duration) => (NodeTween2D)Property(Node2D.PropertyName.Skew, skew, duration);

    /// <summary>
    /// Tweens the global skew.
    /// </summary>
    public NodeTween2D GlobalSkew(float globalSkew, double duration) => (NodeTween2D)Property(Node2D.PropertyName.GlobalSkew, globalSkew, duration);
}
