using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a rectangle shape.
/// </summary>
public class RectArea2D : ShapeArea2D<RectangleShape2D>
{
    /// <summary>
    /// Gets or sets the rectangle size.
    /// </summary>
    public Vector2 Size
    {
        get => Shape.Size;
        set => Shape.Size = value;
    }

    /// <summary>
    /// Creates a rectangle area with the provided size.
    /// </summary>
    internal RectArea2D(Vector2 size) : base(new RectangleShape2D { Size = size }) 
    { 
    }
}
