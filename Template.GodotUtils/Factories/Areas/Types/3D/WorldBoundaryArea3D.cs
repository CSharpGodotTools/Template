using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a world boundary shape.
/// </summary>
public class WorldBoundaryArea3D : ShapeArea3D<WorldBoundaryShape3D>
{
    /// <summary>
    /// Gets or sets the boundary plane.
    /// </summary>
    public Plane Plane
    {
        get => Shape.Plane;
        set => Shape.Plane = value;
    }

    /// <summary>
    /// Creates a world boundary area with the provided plane.
    /// </summary>
    internal WorldBoundaryArea3D(Plane plane) : base(new WorldBoundaryShape3D { Plane = plane })
    {
    }
}
