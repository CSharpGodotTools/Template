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
    public static WorldBoundaryArea2D WorldBoundary(Vector2 normal) => new(normal);

    /// <summary>
    /// Creates a capsule area.
    /// </summary>
    public static CapsuleArea2D Capsule(float radius, float height) => new(radius, height);

    /// <summary>
    /// Creates a circle area.
    /// </summary>
    public static CircleArea2D Circle(float radius) => new(radius);

    /// <summary>
    /// Creates a rectangle area.
    /// </summary>
    public static RectArea2D Rect(Vector2 size) => new(size);
}
