using Godot;

namespace GodotUtils;

/// <summary>
/// Area wrapper for a box shape.
/// </summary>
public class BoxArea3D : ShapeArea3D<BoxShape3D>
{
    /// <summary>
    /// Gets or sets the box size.
    /// </summary>
    public Vector3 Size
    {
        get => Shape.Size;
        set => Shape.Size = value;
    }

    /// <summary>
    /// Creates a box area with the provided size.
    /// </summary>
    internal BoxArea3D(Vector3 size) : base(new BoxShape3D { Size = size }) { }
}
