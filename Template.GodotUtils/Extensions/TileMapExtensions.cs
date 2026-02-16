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
    public static T GetCustomData<[MustBeVariant]T>(this TileMapLayer tileMap, Vector2I tileCoordinates, string customDataLayerName)
    {
        TileData tileData = tileMap.GetCellTileData(tileCoordinates);

        if (tileData == null)
            return default;

        T data = tileData.GetCustomData(customDataLayerName).As<T>();
        return data;
    }
}
