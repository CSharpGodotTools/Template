using Godot;

namespace GodotUtils;

/// <summary>
/// Base class for 2D shape areas.
/// </summary>
public abstract class ShapeArea2D<TShape> : BaseShapeArea<Area2D> where TShape : Shape2D
{
    protected readonly TShape Shape;
    private readonly CollisionShape2D _collision;

    /// <summary>
    /// Creates a shape area backed by the provided shape.
    /// </summary>
    protected ShapeArea2D(TShape shape) : base(new Area2D())
    {
        Shape = shape;
        _collision = new CollisionShape2D { Shape = Shape };
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
