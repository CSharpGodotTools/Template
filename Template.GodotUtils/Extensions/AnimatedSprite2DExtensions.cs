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
    /// <param name="sprite">Animated sprite to update.</param>
    /// <param name="anim">Animation name to play.</param>
    public static void InstantPlay(this AnimatedSprite2D sprite, string anim)
    {
        sprite.Animation = anim;
        sprite.Play(anim);
    }

    /// <summary>
    /// Plays an animation immediately at the provided frame.
    /// </summary>
    /// <param name="sprite">Animated sprite to update.</param>
    /// <param name="anim">Animation name to play.</param>
    /// <param name="frame">Frame index to set before playback starts.</param>
    public static void InstantPlay(this AnimatedSprite2D sprite, string anim, int frame)
    {
        sprite.Animation = anim;

        int frameCount = sprite.SpriteFrames.GetFrameCount(anim);

        // Apply requested frame only when index is within animation bounds.
        if (frameCount - 1 >= frame)
        {
            sprite.Frame = frame;
        }
        // Log out-of-range frame selection for debugging.
        else
        {
            GD.Print($"The frame '{frame}' specified for {sprite.Name} is " +
                $"out of range (frame count is '{frameCount}')");
        }

        sprite.Play(anim);
    }

    /// <summary>
    /// Plays an animation starting at a random frame.
    /// </summary>
    /// <param name="sprite">Animated sprite to update.</param>
    /// <param name="anim">Optional animation name, or empty to use the current animation.</param>
    public static void PlayRandom(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);
        sprite.InstantPlay(resolvedAnim);
        sprite.Frame = GD.RandRange(0, sprite.SpriteFrames.GetFrameCount(resolvedAnim) - 1);
    }

    /// <summary>
    /// Gets the unscaled size of the first frame.
    /// </summary>
    /// <param name="sprite">Animated sprite that owns the animation frames.</param>
    /// <param name="anim">Optional animation name, or empty to use the current animation.</param>
    /// <returns>Width and height of frame zero before node scaling.</returns>
    public static Vector2 GetSize(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);

        Texture2D frameTexture = sprite.SpriteFrames.GetFrameTexture(resolvedAnim, 0);

        return new Vector2(frameTexture.GetWidth(), frameTexture.GetHeight());
    }

    /// <summary>
    /// Gets the scaled size of the first frame.
    /// </summary>
    /// <param name="sprite">Animated sprite that owns the animation frames.</param>
    /// <param name="anim">Optional animation name, or empty to use the current animation.</param>
    /// <returns>Width and height of frame zero after node scaling.</returns>
    public static Vector2 GetScaledSize(this AnimatedSprite2D sprite, string anim = "")
    {
        Vector2 size = sprite.GetSize(anim);

        return new Vector2(size.X * sprite.Scale.X, size.Y * sprite.Scale.Y);
    }

    /// <summary>
    /// Gets the visible pixel size after trimming transparent borders.
    /// </summary>
    /// <param name="sprite">Animated sprite that owns the animation frames.</param>
    /// <param name="anim">Optional animation name, or empty to use the current animation.</param>
    /// <returns>Visible pixel width and height after transparency trimming.</returns>
    public static Vector2 GetPixelSize(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);
        return new Vector2(GetPixelWidth(sprite, resolvedAnim), GetPixelHeight(sprite, resolvedAnim));
    }

    /// <summary>
    /// Gets the visible pixel width after trimming transparent columns.
    /// </summary>
    /// <param name="sprite">Animated sprite that owns the animation frames.</param>
    /// <param name="anim">Optional animation name, or empty to use the current animation.</param>
    /// <returns>Visible width in pixels after transparency trimming and scaling.</returns>
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
    /// <param name="sprite">Animated sprite that owns the animation frames.</param>
    /// <param name="anim">Optional animation name, or empty to use the current animation.</param>
    /// <returns>Visible height in pixels after transparency trimming and scaling.</returns>
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
    /// <param name="sprite">Animated sprite that owns the animation frames.</param>
    /// <param name="anim">Optional animation name, or empty to use the current animation.</param>
    /// <returns>Pixel distance from the frame bottom to the first opaque center pixel.</returns>
    public static int GetPixelBottomY(this AnimatedSprite2D sprite, string anim = "")
    {
        string resolvedAnim = ResolveAnimation(sprite, anim);
        Image img = GetFrameImage(sprite, resolvedAnim, out Vector2I size);

        // Might not work with all sprites but works with ninja.
        // The -2 offset that is
        int diff = 0;

        for (int y = size.Y - 1; y >= 0; y--)
        {
            // Stop once an opaque pixel is reached in center column.
            if (img.GetPixel(size.X / 2, y).A != 0)
                break;

            diff++;
        }

        return diff;
    }

    /// <summary>
    /// Returns the requested animation name or falls back to the sprite's current animation.
    /// </summary>
    /// <param name="sprite">Animated sprite that provides the fallback animation.</param>
    /// <param name="anim">Requested animation name.</param>
    /// <returns>Resolved animation name to use for frame lookups.</returns>
    private static string ResolveAnimation(AnimatedSprite2D sprite, string anim)
    {
        return string.IsNullOrWhiteSpace(anim) ? sprite.Animation : anim;
    }

    /// <summary>
    /// Gets the first frame image for an animation and outputs its dimensions.
    /// </summary>
    /// <param name="sprite">Animated sprite that owns the frame set.</param>
    /// <param name="anim">Animation name used for frame retrieval.</param>
    /// <param name="size">Resolved frame image dimensions.</param>
    /// <returns>Image for frame zero of the resolved animation.</returns>
    private static Image GetFrameImage(AnimatedSprite2D sprite, string anim, out Vector2I size)
    {
        Texture2D tex = sprite.SpriteFrames.GetFrameTexture(anim, 0);
        Image img = tex.GetImage();
        size = img.GetSize();
        return img;
    }
}
