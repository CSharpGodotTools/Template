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
    public static Vector2 GetScaledSize(this Sprite2D sprite)
    {
        return sprite.GetSize() * sprite.Scale;
    }

    /// <summary>
    /// Gets the unscaled texture size.
    /// </summary>
    public static Vector2 GetSize(this Sprite2D sprite)
    {
        return sprite.Texture.GetSize();
    }

    /// <summary>
    /// Gets the visible pixel size after trimming transparent borders.
    /// </summary>
    public static Vector2 GetPixelSize(this Sprite2D sprite)
    {
        return new(GetPixelWidth(sprite), GetPixelHeight(sprite));
    }

    /// <summary>
    /// Gets the visible pixel width after trimming transparent columns.
    /// </summary>
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
    public static int GetPixelBottomY(this Sprite2D sprite)
    {
        Image img = GetTextureImage(sprite, out Vector2I size);

        // Might not work with all sprites but works with ninja.
        // The -2 offset that is
        int diff = 0;

        for (int y = size.Y - 1; y >= 0; y--)
        {
            if (img.GetPixel(size.X / 2, y).A != 0)
            {
                break;
            }

            diff++;
        }

        return diff;
    }

    private static Image GetTextureImage(Sprite2D sprite, out Vector2I size)
    {
        Image img = sprite.Texture.GetImage();
        size = img.GetSize();
        return img;
    }
}
