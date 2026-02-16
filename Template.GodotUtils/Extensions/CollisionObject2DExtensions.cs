using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for 2D collision objects.
/// </summary>
public static class CollisionObject2DExtensions
{
    /// <summary>
    /// Sets collision layers from the provided layer indices.
    /// </summary>
    public static void SetCollisionLayers(this CollisionObject2D collisionObject, params int[] layers)
    {
        collisionObject.CollisionLayer = (uint)MathUtils.GetLayerValues(layers);
    }

    /// <summary>
    /// Sets collision masks from the provided layer indices.
    /// </summary>
    public static void SetCollisionMasks(this CollisionObject2D collisionObject, params int[] layers)
    {
        collisionObject.CollisionMask = (uint)MathUtils.GetLayerValues(layers);
    }

    /// <summary>
    /// Clears all collision layers.
    /// </summary>
    public static void ClearCollisionLayers(this CollisionObject2D collisionObject2D)
    {
        collisionObject2D.CollisionLayer = 0;
    }

    /// <summary>
    /// Clears all collision masks.
    /// </summary>
    public static void ClearCollisionMasks(this CollisionObject2D collisionObject2D)
    {
        collisionObject2D.CollisionMask = 0;
    }
}
