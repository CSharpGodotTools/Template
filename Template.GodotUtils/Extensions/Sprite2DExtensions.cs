using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for Sprite2D.
/// </summary>
public static class Sprite2DExtensions
{
    /// <summary>
    /// Gets the size multiplied by the sprite scale.
    /// </summary>
    /// <param name="sprite">Sprite whose scaled size is requested.</param>
    /// <returns>Texture size multiplied by sprite scale.</returns>
    public static Vector2 GetScaledSize(this Sprite2D sprite)
    {
        return sprite.GetSize() * sprite.Scale;
    }

    /// <summary>
    /// Gets the unscaled texture size.
    /// </summary>
    /// <param name="sprite">Sprite whose texture size is requested.</param>
    /// <returns>Raw texture size in pixels.</returns>
    public static Vector2 GetSize(this Sprite2D sprite)
    {
        return sprite.Texture.GetSize();
    }

    /// <summary>
    /// Gets the visible pixel size after trimming transparent borders.
    /// </summary>
    /// <param name="sprite">Sprite whose visible pixel size is requested.</param>
    /// <returns>Visible width and height after transparency trimming.</returns>
    public static Vector2 GetPixelSize(this Sprite2D sprite)
    {
        return new(GetPixelWidth(sprite), GetPixelHeight(sprite));
    }

    /// <summary>
    /// Gets the visible pixel width after trimming transparent columns.
    /// </summary>
    /// <param name="sprite">Sprite whose visible pixel width is requested.</param>
    /// <returns>Visible width in pixels after transparency trimming and scaling.</returns>
    public static int GetPixelWidth(this Sprite2D sprite)
    {
        Image img = GetTextureImage(sprite, out Vector2I size);

        int transColumnsLeft = ImageUtils.GetTransparentColumnsLeft(img, size);
        int transColumnsRight = ImageUtils.GetTransparentColumnsRight(img, size);

        int pixelWidth = size.X - transColumnsLeft - transColumnsRight;

        return (int)(pixelWidth * sprite.Scale.X);
    }

    /// <summary>
    /// Gets the visible pixel height after trimming transparent rows.
    /// </summary>
    /// <param name="sprite">Sprite whose visible pixel height is requested.</param>
    /// <returns>Visible height in pixels after transparency trimming and scaling.</returns>
    public static int GetPixelHeight(this Sprite2D sprite)
    {
        Image img = GetTextureImage(sprite, out Vector2I size);

        int transRowsTop = ImageUtils.GetTransparentRowsTop(img, size);
        int transRowsBottom = ImageUtils.GetTransparentRowsBottom(img, size);

        int pixelHeight = size.Y - transRowsTop - transRowsBottom;

        return (int)(pixelHeight * sprite.Scale.Y);
    }

    /// <summary>
    /// Gets the offset from the bottom to the first opaque pixel.
    /// </summary>
    /// <param name="sprite">Sprite whose bottom transparent offset is requested.</param>
    /// <returns>Pixel distance from the bottom edge to the first opaque center pixel.</returns>
    public static int GetPixelBottomY(this Sprite2D sprite)
    {
        Image img = GetTextureImage(sprite, out Vector2I size);

        // Scans from the bottom of the sprite upward along the vertical center column (size.X / 2)
        // to find how many fully-transparent rows exist at the bottom.
        // Assumption: the sprite's opaque area passes through the center column — may give wrong
        // results for asymmetric sprites.
        int diff = 0;

        for (int y = size.Y - 1; y >= 0; y--)
        {
            // Stop once an opaque pixel is reached in center column.
            if (img.GetPixel(size.X / 2, y).A != 0)
            {
                break;
            }

            diff++;
        }

        return diff;
    }

    /// <summary>
    /// Retrieves the sprite texture image and returns its size.
    /// </summary>
    /// <param name="sprite">Sprite whose texture image is requested.</param>
    /// <param name="size">Texture size in pixels.</param>
    /// <returns>Image data for the sprite texture.</returns>
    private static Image GetTextureImage(Sprite2D sprite, out Vector2I size)
    {
        Image img = sprite.Texture.GetImage();
        size = img.GetSize();
        return img;
    }
}
