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
    public static void EnableCollisionLayers(this CollisionObject2D collisionObject, params int[] layers)
    {
        collisionObject.CollisionLayer = (uint)MathUtils.GetLayerValues(layers);
    }

    /// <summary>
    /// Disable ALL collision layers, then enable the specified layers.
    /// </summary>
    public static void EnableCollisionLayers(this CollisionObject2D collisionObject, string layers)
    {
        collisionObject.EnableCollisionLayers(ConvertToUniqueIntArray(layers));
    }

    /// <summary>
    /// Disable ALL mask layers, then enable the specified layers.
    /// </summary>
    public static void EnableCollisionMasks(this CollisionObject2D collisionObject, params int[] layers)
    {
        collisionObject.CollisionMask = (uint)MathUtils.GetLayerValues(layers);
    }

    /// <summary>
    /// Disable ALL mask layers, then enable the specified layers.
    /// </summary>
    public static void EnableCollisionMasks(this CollisionObject2D collisionObject, string layers)
    {
        collisionObject.EnableCollisionMasks(ConvertToUniqueIntArray(layers));
    }

    /// <summary>
    /// Disable ALL collision layers.
    /// </summary>
    public static void DisableAllCollisionLayers(this CollisionObject2D collisionObject2D)
    {
        collisionObject2D.CollisionLayer = 0;
    }

    /// <summary>
    /// Disable ALL collision masks.
    /// </summary>
    public static void DisableAllCollisionMasks(this CollisionObject2D collisionObject2D)
    {
        collisionObject2D.CollisionMask = 0;
    }

    /// <summary>
    /// Converts a string to a unique int array. For example "1223" becomes [1, 2, 3].
    /// </summary>
    private static int[] ConvertToUniqueIntArray(string numberString)
    {
        if (string.IsNullOrEmpty(numberString))
            return [];
        
        var uniqueNumbers = new HashSet<int>();
        
        foreach (char c in numberString)
        {
            if (char.IsDigit(c))
            {
                uniqueNumbers.Add(int.Parse(c.ToString()));
            }
        }
        
        return [.. uniqueNumbers];
    }
}
