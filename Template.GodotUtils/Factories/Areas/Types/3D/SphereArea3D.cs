using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a sphere shape.
/// </summary>
public class SphereArea3D : ShapeArea3D<SphereShape3D>
{
    /// <summary>
    /// Gets or sets the sphere radius.
    /// </summary>
    public float Radius
    {
        get => Shape.Radius;
        set => Shape.Radius = value;
    }

    /// <summary>
    /// Creates a sphere area with the provided radius.
    /// </summary>
    internal SphereArea3D(float radius) : base(new SphereShape3D { Radius = radius })
    {
    }
}
