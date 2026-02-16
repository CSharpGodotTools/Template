using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for 3D areas.
/// </summary>
public static class AreaFactory3D
{
    /// <summary>
    /// Creates a world boundary area.
    /// </summary>
    public static WorldBoundaryArea3D WorldBoundary(Plane plane) => new(plane);

    /// <summary>
    /// Creates a cylinder area.
    /// </summary>
    public static CylinderArea3D Cylinder(float radius, float height) => new(radius, height);

    /// <summary>
    /// Creates a capsule area.
    /// </summary>
    public static CapsuleArea3D Capsule(float radius, float height) => new(radius, height);

    /// <summary>
    /// Creates a sphere area.
    /// </summary>
    public static SphereArea3D Sphere(float radius) => new(radius);

    /// <summary>
    /// Creates a box area.
    /// </summary>
    public static BoxArea3D Box(Vector3 size) => new(size);
}
