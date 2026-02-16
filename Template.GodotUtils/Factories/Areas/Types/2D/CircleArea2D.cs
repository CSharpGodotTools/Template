using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a circle shape.
/// </summary>
public class CircleArea2D : ShapeArea2D<CircleShape2D>
{
    /// <summary>
    /// Gets or sets the circle radius.
    /// </summary>
    public float Radius
    {
        get => Shape.Radius;
        set => Shape.Radius = value;
    }

    /// <summary>
    /// Creates a circle area with the provided radius.
    /// </summary>
    internal CircleArea2D(float radius) : base(new CircleShape2D { Radius = radius }) 
    {
    }
}
