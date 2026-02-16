using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for RayCast3D.
/// </summary>
public static class RayCast3DExtensions
{
    /// <summary>
    /// Returns the number of raycasts currently colliding.
    /// </summary>
    public static int GetRaycastsColliding(this RayCast3D[] raycasts)
    {
        int numRaycastsColliding = 0;

        foreach (RayCast3D raycast in raycasts)
        {
            if (raycast.IsColliding())
            {
                numRaycastsColliding++;
            }
        }

        return numRaycastsColliding;
    }

    /// <summary>
    /// Excludes all parent collision objects from the raycast.
    /// </summary>
    public static void ExcludeRaycastParents(this RayCast3D raycast)
    {
        ExcludeParents(raycast, raycast.GetParent());
    }

    private static void ExcludeParents(RayCast3D raycast, Node parent)
    {
        if (parent == null)
            return;

        if (parent is CollisionObject3D collision)
            raycast.AddException(collision);

        ExcludeParents(raycast, parent.GetParentOrNull<Node>());
    }
}
