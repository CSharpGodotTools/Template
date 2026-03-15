using Godot;

namespace GodotUtils;

/// <summary>
/// Base class for 3D shape areas that directly expose the full Area3D API.
/// </summary>
public abstract class ShapeArea3D<TShape> : Area3D where TShape : Shape3D
{
    /// <summary>
    /// Gets the backing shape resource.
    /// </summary>
    protected TShape Shape { get; }

    /// <summary>
    /// Gets the collision node attached to this area.
    /// </summary>
    public CollisionShape3D Collision { get; }

    /// <summary>
    /// Creates a shape area backed by the provided shape.
    /// </summary>
    protected ShapeArea3D(TShape shape)
    {
        Shape = shape;
        Collision = new CollisionShape3D { Shape = Shape };
        AddChild(Collision);
    }

    /// <summary>
    /// Sets the debug color for the collision shape.
    /// </summary>
    public void SetColor(Color color, bool transparent = false)
    {
        color.A = transparent ? 0f : 1f;
        Collision.DebugColor = color;
    }

    /// <summary>
    /// Gets the debug color for the collision shape.
    /// </summary>
    public Color GetColor() => Collision.DebugColor;
}
