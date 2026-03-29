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
        RayCastParentTraversal.ForEachParent(raycast.GetParent(), node =>
        {
            if (node is CollisionObject3D collision)
                raycast.AddException(collision);
        });
    }
}
