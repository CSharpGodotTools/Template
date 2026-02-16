using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a capsule shape.
/// </summary>
public class CapsuleArea2D : ShapeArea2D<CapsuleShape2D>
{
    /// <summary>
    /// Gets or sets the capsule radius.
    /// </summary>
    public float Radius
    {
        get => Shape.Radius;
        set => Shape.Radius = value;
    }

    /// <summary>
    /// Gets or sets the capsule height.
    /// </summary>
    public float Height
    {
        get => Shape.Height;
        set => Shape.Height = value;
    }

    /// <summary>
    /// Creates a capsule area with the provided radius and height.
    /// </summary>
    internal CapsuleArea2D(float radius, float height) : base(new CapsuleShape2D { Radius = radius, Height = height })
    {
    }
}
