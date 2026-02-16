using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a 2D world boundary shape.
/// </summary>
public class WorldBoundaryArea2D : ShapeArea2D<WorldBoundaryShape2D>
{
    /// <summary>
    /// Gets or sets the boundary normal.
    /// </summary>
    public Vector2 Normal
    {
        get => Shape.Normal;
        set => Shape.Normal = value;
    }

    /// <summary>
    /// Creates a world boundary area with the provided normal.
    /// </summary>
    internal WorldBoundaryArea2D(Vector2 normal) : base(new WorldBoundaryShape2D { Normal = normal })
    {
    }
}
