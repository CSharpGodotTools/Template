using Godot;

namespace GodotUtils;

/// <summary>
/// Image transparency scanning helpers used by sprite-size utilities.
/// </summary>
public static class ImageUtils
{
    /// <summary>
    /// Returns the number of fully transparent columns on the left side of the image.
    /// </summary>
    /// <param name="img">Image to scan.</param>
    /// <param name="size">Image size in pixels.</param>
    /// <returns>Count of transparent columns from the left edge.</returns>
    public static int GetTransparentColumnsLeft(Image img, Vector2 size)
    {
        int width = (int)size.X;
        int height = (int)size.Y;
        return CountTransparentColumns(img, width, height, 0, 1);
    }

    /// <summary>
    /// Returns the number of fully transparent columns on the right side of the image.
    /// </summary>
    /// <param name="img">Image to scan.</param>
    /// <param name="size">Image size in pixels.</param>
    /// <returns>Count of transparent columns from the right edge.</returns>
    public static int GetTransparentColumnsRight(Image img, Vector2 size)
    {
        int width = (int)size.X;
        int height = (int)size.Y;
        return CountTransparentColumns(img, width, height, width - 1, -1);
    }

    /// <summary>
    /// Returns the number of fully transparent rows on the top of the image.
    /// </summary>
    /// <param name="img">Image to scan.</param>
    /// <param name="size">Image size in pixels.</param>
    /// <returns>Count of transparent rows from the top edge.</returns>
    public static int GetTransparentRowsTop(Image img, Vector2 size)
    {
        int width = (int)size.X;
        int height = (int)size.Y;
        return CountTransparentRows(img, width, height, 0, 1);
    }

    /// <summary>
    /// Returns the number of fully transparent rows on the bottom of the image.
    /// </summary>
    /// <param name="img">Image to scan.</param>
    /// <param name="size">Image size in pixels.</param>
    /// <returns>Count of transparent rows from the bottom edge.</returns>
    public static int GetTransparentRowsBottom(Image img, Vector2 size)
    {
        int width = (int)size.X;
        int height = (int)size.Y;
        return CountTransparentRows(img, width, height, height - 1, -1);
    }

    /// <summary>
    /// Counts fully transparent columns while scanning horizontally from a starting x index.
    /// </summary>
    /// <param name="img">Image to scan.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="startX">First x coordinate to inspect.</param>
    /// <param name="step">Horizontal scan direction, typically 1 or -1.</param>
    /// <returns>Number of consecutive transparent columns encountered.</returns>
    private static int CountTransparentColumns(Image img, int width, int height, int startX, int step)
    {
        int columns = 0;

        // Scan columns until we hit the first non-transparent pixel.
        for (int x = startX; x >= 0 && x < width; x += step)
        {
            for (int y = 0; y < height; y++)
            {
                // Stop at first opaque pixel in this scan direction.
                if (img.GetPixel(x, y).A != 0)
                    return columns;
            }

            columns++;
        }

        return columns;
    }

    /// <summary>
    /// Counts fully transparent rows while scanning vertically from a starting y index.
    /// </summary>
    /// <param name="img">Image to scan.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="startY">First y coordinate to inspect.</param>
    /// <param name="step">Vertical scan direction, typically 1 or -1.</param>
    /// <returns>Number of consecutive transparent rows encountered.</returns>
    private static int CountTransparentRows(Image img, int width, int height, int startY, int step)
    {
        int rows = 0;

        // Scan rows until we hit the first non-transparent pixel.
        for (int y = startY; y >= 0 && y < height; y += step)
        {
            for (int x = 0; x < width; x++)
            {
                // Stop at first opaque pixel in this scan direction.
                if (img.GetPixel(x, y).A != 0)
                    return rows;
            }

            rows++;
        }

        return rows;
    }
}
