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
    public static Variant GetTileData(this RayCast2D raycast, string layerName)
    {
        if (!raycast.IsColliding() || raycast.GetCollider() is not TileMapLayer tileMap)
            return default;

        Vector2 collisionPos = raycast.GetCollisionPoint();
        Vector2I tilePos = tileMap.LocalToMap(tileMap.ToLocal(collisionPos));

        TileData tileData = tileMap.GetCellTileData(tilePos);

        if (tileData == null)
            return default;

        return tileData.GetCustomData(layerName);
    }

    /// <summary>
    /// Sets only the provided collision mask values to true.
    /// </summary>
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
    public static void ExcludeRaycastParents(this RayCast2D raycast, Node parent)
    {
        if (parent == null)
            return;

        if (parent is CollisionObject2D collision)
            raycast.AddException(collision);

        ExcludeRaycastParents(raycast, parent.GetParentOrNull<Node>());
    }

    /// <summary>
    /// Returns true if any raycast in the collection is colliding.
    /// </summary>
    public static bool IsAnyRayCastColliding(this RayCast2D[] raycasts)
    {
        foreach (RayCast2D raycast in raycasts)
        {
            if (raycast.IsColliding())
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the first colliding raycast in the collection.
    /// </summary>
    public static RayCast2D GetAnyRayCastCollider(this RayCast2D[] raycasts)
    {
        foreach (RayCast2D raycast in raycasts)
        {
            if (raycast.IsColliding())
                return raycast;
        }

        return default;
    }
}
