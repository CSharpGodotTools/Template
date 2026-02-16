using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a cylinder shape.
/// </summary>
public class CylinderArea3D : ShapeArea3D<CylinderShape3D>
{
    /// <summary>
    /// Gets or sets the cylinder radius.
    /// </summary>
    public float Radius
    {
        get => Shape.Radius;
        set => Shape.Radius = value;
    }

    /// <summary>
    /// Gets or sets the cylinder height.
    /// </summary>
    public float Height
    {
        get => Shape.Height;
        set => Shape.Height = value;
    }

    /// <summary>
    /// Creates a cylinder area with the provided radius and height.
    /// </summary>
    internal CylinderArea3D(float radius, float height) : base(new CylinderShape3D { Radius = radius, Height = height })
    {
    }
}
