using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for RayCast2D.
/// </summary>
public static class RayCast2DExtensions
{
    /// <summary>
    /// Gets custom tile data from the tilemap layer the raycast hits.
    /// </summary>
    /// <param name="raycast">Raycast used for collision lookup.</param>
    /// <param name="layerName">Custom data layer name to read from the hit tile.</param>
    /// <returns>Custom data value when available; otherwise default.</returns>
    public static Variant GetTileData(this RayCast2D raycast, string layerName)
    {
        // Tile data lookup only applies to colliding raycasts hitting TileMapLayer.
        if (!raycast.IsColliding() || raycast.GetCollider() is not TileMapLayer tileMap)
            return default;

        Vector2 collisionPos = raycast.GetCollisionPoint();
        Vector2I tilePos = tileMap.LocalToMap(tileMap.ToLocal(collisionPos));

        TileData tileData = tileMap.GetCellTileData(tilePos);

        // Return default when tile data is missing at collision cell.
        if (tileData == null)
            return default;

        return tileData.GetCustomData(layerName);
    }

    /// <summary>
    /// Sets only the provided collision mask values to true.
    /// </summary>
    /// <param name="node">Raycast whose collision mask should be updated.</param>
    /// <param name="values">Collision layer values to enable.</param>
    public static void SetCollisionMask(this RayCast2D node, params int[] values)
    {
        // Reset all mask values to 0
        node.CollisionMask = 0;

        foreach (int value in values)
        {
            node.SetCollisionMaskValue(value, true);
        }
    }

    /// <summary>
    /// Excludes all parent CollisionObject2D nodes from the raycast.
    /// </summary>
    /// <param name="raycast">Raycast that should ignore parent colliders.</param>
    /// <param name="parent">Node whose parent chain is traversed for exclusions.</param>
    public static void ExcludeRaycastParents(this RayCast2D raycast, Node parent)
    {
        RayCastParentTraversal.ForEachParent(parent, node =>
        {
            // Exclude parent collision objects from hit detection.
            if (node is CollisionObject2D collision)
                raycast.AddException(collision);
        });
    }

    /// <summary>
    /// Returns true if any raycast in the collection is colliding.
    /// </summary>
    /// <param name="raycasts">Raycasts to inspect.</param>
    /// <returns><see langword="true"/> when any raycast currently collides.</returns>
    public static bool IsAnyRayCastColliding(this RayCast2D[] raycasts)
    {
        foreach (RayCast2D raycast in raycasts)
        {
            // Return on first hit to avoid unnecessary checks.
            if (raycast.IsColliding())
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the first colliding raycast in the collection.
    /// </summary>
    /// <param name="raycasts">Raycasts to inspect.</param>
    /// <returns>First colliding raycast, or <see langword="null"/> when none collide.</returns>
    public static RayCast2D? GetAnyRayCastCollider(this RayCast2D[] raycasts)
    {
        foreach (RayCast2D raycast in raycasts)
        {
            // Return first colliding raycast.
            if (raycast.IsColliding())
                return raycast;
        }

        return default;
    }
}
