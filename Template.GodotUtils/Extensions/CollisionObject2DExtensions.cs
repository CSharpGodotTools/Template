using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotUtils;

/// <summary>
/// Extension helpers for 2D collision objects.
/// </summary>
public static class CollisionObject2DExtensions
{
    /// <summary>
    /// Disable ALL collision layers, then enable the specified layers.
    /// </summary>
    /// <param name="collisionObject">Collision object whose layer bitmask will be updated.</param>
    /// <param name="layers">Layer indices to enable after resetting all layers.</param>
    public static void EnableCollisionLayers(this CollisionObject2D collisionObject, params int[] layers)
    {
        collisionObject.CollisionLayer = (uint)MathUtils.GetLayerValues(layers);
    }

    /// <summary>
    /// Disable ALL collision layers, then enable the specified layers.
    /// </summary>
    /// <param name="collisionObject">Collision object whose layer bitmask will be updated.</param>
    /// <param name="layers">String of digit characters representing layer indices.</param>
    public static void EnableCollisionLayers(this CollisionObject2D collisionObject, string layers)
    {
        collisionObject.EnableCollisionLayers(ConvertToUniqueIntArray(layers));
    }

    /// <summary>
    /// Disable ALL mask layers, then enable the specified layers.
    /// </summary>
    /// <param name="collisionObject">Collision object whose mask bitmask will be updated.</param>
    /// <param name="layers">Layer indices to enable after resetting all masks.</param>
    public static void EnableCollisionMasks(this CollisionObject2D collisionObject, params int[] layers)
    {
        collisionObject.CollisionMask = (uint)MathUtils.GetLayerValues(layers);
    }

    /// <summary>
    /// Disable ALL mask layers, then enable the specified layers.
    /// </summary>
    /// <param name="collisionObject">Collision object whose mask bitmask will be updated.</param>
    /// <param name="layers">String of digit characters representing layer indices.</param>
    public static void EnableCollisionMasks(this CollisionObject2D collisionObject, string layers)
    {
        collisionObject.EnableCollisionMasks(ConvertToUniqueIntArray(layers));
    }

    /// <summary>
    /// Disable ALL collision layers.
    /// </summary>
    /// <param name="collisionObject2D">Collision object whose layer mask will be cleared.</param>
    public static void DisableAllCollisionLayers(this CollisionObject2D collisionObject2D)
    {
        collisionObject2D.CollisionLayer = 0;
    }

    /// <summary>
    /// Disable ALL collision masks.
    /// </summary>
    /// <param name="collisionObject2D">Collision object whose mask will be cleared.</param>
    public static void DisableAllCollisionMasks(this CollisionObject2D collisionObject2D)
    {
        collisionObject2D.CollisionMask = 0;
    }

    /// <summary>
    /// Converts a string to a unique int array. For example "1223" becomes [1, 2, 3].
    /// </summary>
    /// <param name="numberString">String containing digit characters that represent layer indices.</param>
    /// <returns>Unique set of parsed layer indices.</returns>
    private static int[] ConvertToUniqueIntArray(string numberString)
    {
        // Empty input maps to no layers.
        if (string.IsNullOrEmpty(numberString))
            return [];

        var uniqueNumbers = new HashSet<int>();

        foreach (char c in numberString)
        {
            // Keep only numeric characters as layer indices.
            if (char.IsDigit(c))
                uniqueNumbers.Add(int.Parse(c.ToString()));
        }

        return [.. uniqueNumbers];
    }
}
