using Godot;

namespace GodotUtils;

/// <summary>
/// Base class for 2D shape areas that directly expose the full Area2D API.
/// </summary>
/// <typeparam name="TShape">Concrete 2D shape resource type used by the area.</typeparam>
public abstract class ShapeArea2D<TShape> : Area2D where TShape : Shape2D
{
    /// <summary>
    /// Gets the backing shape resource.
    /// </summary>
    protected TShape Shape { get; }

    /// <summary>
    /// Gets the collision node attached to this area.
    /// </summary>
    public CollisionShape2D Collision { get; }

    /// <summary>
    /// Creates a shape area backed by the provided shape.
    /// </summary>
    /// <param name="shape">Shape resource assigned to the collision node.</param>
    protected ShapeArea2D(TShape shape)
    {
        Shape = shape;
        Collision = new CollisionShape2D { Shape = Shape };
        AddChild(Collision);
    }

    /// <summary>
    /// Sets the debug color for the collision shape.
    /// </summary>
    /// <param name="color">Debug color applied to the collision shape.</param>
    public void SetColor(Color color) => Collision.DebugColor = color;

    /// <summary>
    /// Gets the debug color for the collision shape.
    /// </summary>
    /// <returns>Current collision debug color.</returns>
    public Color GetColor() => Collision.DebugColor;
}
