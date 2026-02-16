using Godot;

namespace GodotUtils;

/// <summary>
/// Base class for 3D shape areas.
/// </summary>
public abstract class ShapeArea3D<TShape> : BaseShapeArea<Area3D> where TShape : Shape3D
{
    protected readonly TShape Shape;
    private readonly CollisionShape3D _collision;

    /// <summary>
    /// Creates a shape area backed by the provided shape.
    /// </summary>
    protected ShapeArea3D(TShape shape) : base(new Area3D())
    {
        Shape = shape;
        _collision = new CollisionShape3D { Shape = Shape };
        Area.AddChild(_collision);
    }

    /// <summary>
    /// Sets the debug color for the collision shape.
    /// </summary>
    public override void SetColor(Color color, bool transparent = false)
    {
        color.A = transparent ? 0f : 1f;
        _collision.DebugColor = color;
    }

    /// <summary>
    /// Gets the debug color for the collision shape.
    /// </summary>
    public override Color GetColor() => _collision.DebugColor;
}
