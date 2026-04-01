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
    /// <param name="plane">Boundary plane used by the world-boundary shape.</param>
    /// <returns>Configured world-boundary area instance.</returns>
    public static WorldBoundaryArea3D WorldBoundary(Plane plane) => new(plane);

    /// <summary>
    /// Creates a cylinder area.
    /// </summary>
    /// <param name="radius">Cylinder radius.</param>
    /// <param name="height">Cylinder height.</param>
    /// <returns>Configured cylinder area instance.</returns>
    public static CylinderArea3D Cylinder(float radius, float height) => new(radius, height);

    /// <summary>
    /// Creates a capsule area.
    /// </summary>
    /// <param name="radius">Capsule radius.</param>
    /// <param name="height">Capsule cylinder height.</param>
    /// <returns>Configured capsule area instance.</returns>
    public static CapsuleArea3D Capsule(float radius, float height) => new(radius, height);

    /// <summary>
    /// Creates a sphere area.
    /// </summary>
    /// <param name="radius">Sphere radius.</param>
    /// <returns>Configured sphere area instance.</returns>
    public static SphereArea3D Sphere(float radius) => new(radius);

    /// <summary>
    /// Creates a box area.
    /// </summary>
    /// <param name="size">Box dimensions.</param>
    /// <returns>Configured box area instance.</returns>
    public static BoxArea3D Box(Vector3 size) => new(size);
}
