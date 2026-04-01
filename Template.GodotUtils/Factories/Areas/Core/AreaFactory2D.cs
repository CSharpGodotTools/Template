using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for 2D areas.
/// </summary>
public static class AreaFactory2D
{
    /// <summary>
    /// Creates a world boundary area.
    /// </summary>
    /// <param name="normal">Boundary normal direction.</param>
    /// <returns>Configured world-boundary area instance.</returns>
    public static WorldBoundaryArea2D WorldBoundary(Vector2 normal) => new(normal);

    /// <summary>
    /// Creates a capsule area.
    /// </summary>
    /// <param name="radius">Capsule radius.</param>
    /// <param name="height">Capsule cylinder height.</param>
    /// <returns>Configured capsule area instance.</returns>
    public static CapsuleArea2D Capsule(float radius, float height) => new(radius, height);

    /// <summary>
    /// Creates a circle area.
    /// </summary>
    /// <param name="radius">Circle radius.</param>
    /// <returns>Configured circle area instance.</returns>
    public static CircleArea2D Circle(float radius) => new(radius);

    /// <summary>
    /// Creates a rectangle area.
    /// </summary>
    /// <param name="size">Rectangle size.</param>
    /// <returns>Configured rectangle area instance.</returns>
    public static RectArea2D Rect(Vector2 size) => new(size);
}
