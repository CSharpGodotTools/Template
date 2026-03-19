using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for 2D collision objects.
/// </summary>
public static class CollisionObject2DExtensions
{
    /// <summary>
    /// Disable ALL collision layers, then enable the specified layers.
    /// </summary>
    public static void EnableCollisionLayers(this CollisionObject2D collisionObject, params int[] layers)
    {
        collisionObject.CollisionLayer = (uint)MathUtils.GetLayerValues(layers);
    }

    /// <summary>
    /// Disable ALL mask layers, then enable the specified layers.
    /// </summary>
    public static void EnableCollisionMasks(this CollisionObject2D collisionObject, params int[] layers)
    {
        collisionObject.CollisionMask = (uint)MathUtils.GetLayerValues(layers);
    }

    /// <summary>
    /// Disable ALL collision layers.
    /// </summary>
    public static void ClearCollisionLayers(this CollisionObject2D collisionObject2D)
    {
        collisionObject2D.CollisionLayer = 0;
    }

    /// <summary>
    /// Disable ALL collision masks.
    /// </summary>
    public static void ClearCollisionMasks(this CollisionObject2D collisionObject2D)
    {
        collisionObject2D.CollisionMask = 0;
    }
}
