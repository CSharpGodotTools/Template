using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for tile maps.
/// </summary>
public static class TileMapExtensions
{
    /// <summary>
    /// Gets custom data of type <typeparamref name="T"/> at the tile coordinates.
    /// </summary>
    /// <typeparam name="T">Expected custom-data value type.</typeparam>
    /// <param name="tileMap">Tile map layer that contains the target cell.</param>
    /// <param name="tileCoordinates">Cell coordinates to inspect.</param>
    /// <param name="customDataLayerName">Custom data layer name to read from the tile.</param>
    /// <returns>Custom data value for the requested cell, or default when tile data is missing.</returns>
    public static T GetCustomData<[MustBeVariant] T>(this TileMapLayer tileMap, Vector2I tileCoordinates, string customDataLayerName)
    {
        TileData tileData = tileMap.GetCellTileData(tileCoordinates);

        // Return default when tile has no data entry.
        if (tileData == null)
            return default!;

        return tileData.GetCustomData(customDataLayerName).As<T>();
    }
}
