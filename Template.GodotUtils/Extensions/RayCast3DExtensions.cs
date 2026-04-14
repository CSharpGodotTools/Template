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
    /// <param name="raycasts">Raycasts to inspect.</param>
    /// <returns>Count of raycasts with active collisions.</returns>
    public static int GetRaycastsColliding(this RayCast3D[] raycasts)
    {
        int numRaycastsColliding = 0;

        foreach (RayCast3D raycast in raycasts)
        {
            // Count each raycast that currently reports a hit.
            if (raycast.IsColliding())
                numRaycastsColliding++;
        }

        return numRaycastsColliding;
    }

    /// <summary>
    /// Excludes all parent collision objects from the raycast.
    /// </summary>
    /// <param name="raycast">Raycast that should ignore parent colliders.</param>
    public static void ExcludeRaycastParents(this RayCast3D raycast)
    {
        RayCastParentTraversal.ForEachParent(raycast.GetParent(), node =>
        {
            // Exclude parent collision objects from hit detection.
            if (node is CollisionObject3D collision)
                raycast.AddException(collision);
        });
    }
}
