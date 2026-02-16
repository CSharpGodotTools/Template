using Godot;

namespace GodotUtils;

public static class ImageUtils
{
    /// <summary>
    /// Returns the number of fully transparent columns on the left side of the image.
    /// </summary>
    public static int GetTransparentColumnsLeft(Image img, Vector2 size)
    {
        int width = (int)size.X;
        int height = (int)size.Y;
        return CountTransparentColumns(img, width, height, 0, 1);
    }

    /// <summary>
    /// Returns the number of fully transparent columns on the right side of the image.
    /// </summary>
    public static int GetTransparentColumnsRight(Image img, Vector2 size)
    {
        int width = (int)size.X;
        int height = (int)size.Y;
        return CountTransparentColumns(img, width, height, width - 1, -1);
    }

    /// <summary>
    /// Returns the number of fully transparent rows on the top of the image.
    /// </summary>
    public static int GetTransparentRowsTop(Image img, Vector2 size)
    {
        int width = (int)size.X;
        int height = (int)size.Y;
        return CountTransparentRows(img, width, height, 0, 1);
    }

    /// <summary>
    /// Returns the number of fully transparent rows on the bottom of the image.
    /// </summary>
    public static int GetTransparentRowsBottom(Image img, Vector2 size)
    {
        int width = (int)size.X;
        int height = (int)size.Y;
        return CountTransparentRows(img, width, height, height - 1, -1);
    }

    private static int CountTransparentColumns(Image img, int width, int height, int startX, int step)
    {
        int columns = 0;

        // Scan columns until we hit the first non-transparent pixel.
        for (int x = startX; x >= 0 && x < width; x += step)
        {
            for (int y = 0; y < height; y++)
            {
                if (img.GetPixel(x, y).A != 0)
                    return columns;
            }

            columns++;
        }

        return columns;
    }

    private static int CountTransparentRows(Image img, int width, int height, int startY, int step)
    {
        int rows = 0;

        // Scan rows until we hit the first non-transparent pixel.
        for (int y = startY; y >= 0 && y < height; y += step)
        {
            for (int x = 0; x < width; x++)
            {
                if (img.GetPixel(x, y).A != 0)
                    return rows;
            }

            rows++;
        }

        return rows;
    }
}
