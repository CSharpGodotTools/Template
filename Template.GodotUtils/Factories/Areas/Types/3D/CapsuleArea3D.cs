using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a capsule shape.
/// </summary>
public class CapsuleArea3D : ShapeArea3D<CapsuleShape3D>
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
    internal CapsuleArea3D(float radius, float height) : base(new CapsuleShape3D { Radius = radius, Height = height })
    {
    }
}
