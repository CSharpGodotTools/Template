using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for AnimatedSprite2D.
/// </summary>
public static class AnimatedSprite2DExtensions
{
    /// <summary>
    /// Plays an animation immediately without the usual switch delay.
    /// </summary>
    public static void InstantPlay(this AnimatedSprite2D sprite, string anim)
    {
        sprite.Animation = anim;
        sprite.Play(anim);
    }

    /// <summary>
    /// Plays an animation immediately at the provided frame.
    /// </summary>
    public static void InstantPlay(this AnimatedSprite2D sprite, string anim, int frame)
    {
        sprite.Animation = anim;

        int frameCount = sprite.SpriteFrames.GetFrameCount(anim);

        if (frameCount - 1 >= frame)
        {
            sprite.Frame = frame;
        }
        else
        {
            GD.Print($"The frame '{frame}' specified for {sprite.Name} is " +
                $"lower than the frame count '{frameCount}'");
        }

        sprite.Play(anim);
    }

    /// <summary>
    /// Plays an animation starting at a random frame.
    /// </summary>
    public static void PlayRandom(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);
        sprite.InstantPlay(resolvedAnim);
        sprite.Frame = GD.RandRange(0, sprite.SpriteFrames.GetFrameCount(resolvedAnim));
    }

    /// <summary>
    /// Gets the unscaled size of the first frame.
    /// </summary>
    public static Vector2 GetSize(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);

        Texture2D frameTexture = sprite.SpriteFrames.GetFrameTexture(resolvedAnim, 0);

        return new Vector2(frameTexture.GetWidth(), frameTexture.GetHeight());
    }

    /// <summary>
    /// Gets the scaled size of the first frame.
    /// </summary>
    public static Vector2 GetScaledSize(this AnimatedSprite2D sprite, string anim = "")
    {
        Vector2 size = sprite.GetSize(anim);

        return new Vector2(size.X * sprite.Scale.X, size.Y * sprite.Scale.Y);
    }

    /// <summary>
    /// Gets the visible pixel size after trimming transparent borders.
    /// </summary>
    public static Vector2 GetPixelSize(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);
        return new Vector2(GetPixelWidth(sprite, resolvedAnim), GetPixelHeight(sprite, resolvedAnim));
    }

    /// <summary>
    /// Gets the visible pixel width after trimming transparent columns.
    /// </summary>
    public static int GetPixelWidth(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);
        Image img = GetFrameImage(sprite, resolvedAnim, out Vector2I size);

        int transColumnsLeft = ImageUtils.GetTransparentColumnsLeft(img, size);
        int transColumnsRight = ImageUtils.GetTransparentColumnsRight(img, size);

        int pixelWidth = size.X - transColumnsLeft - transColumnsRight;

        return (int)(pixelWidth * sprite.Scale.X);
    }

    /// <summary>
    /// Gets the visible pixel height after trimming transparent rows.
    /// </summary>
    public static int GetPixelHeight(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);
        Image img = GetFrameImage(sprite, resolvedAnim, out Vector2I size);

        int transRowsTop = ImageUtils.GetTransparentRowsTop(img, size);
        int transRowsBottom = ImageUtils.GetTransparentRowsBottom(img, size);

        int pixelHeight = size.Y - transRowsTop - transRowsBottom;

        return (int)(pixelHeight * sprite.Scale.Y);
    }

    /// <summary>
    /// Gets the offset from the bottom to the first opaque pixel.
    /// </summary>
    public static int GetPixelBottomY(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);
        Image img = GetFrameImage(sprite, resolvedAnim, out Vector2I size);

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

    private static string ResolveAnimation(AnimatedSprite2D sprite, string anim)
    {
        return string.IsNullOrWhiteSpace(anim) ? sprite.Animation : anim;
    }

    private static Image GetFrameImage(AnimatedSprite2D sprite, string anim, out Vector2I size)
    {
        Texture2D tex = sprite.SpriteFrames.GetFrameTexture(anim, 0);
        Image img = tex.GetImage();
        size = img.GetSize();
        return img;
    }
}
