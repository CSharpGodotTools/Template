using Godot;

namespace GodotUtils;

/// <summary>
/// Color helper functions.
/// </summary>
public static class ColorUtils
{
    private const float MaxHue = 359f;
    private const float MaxSaturation = 100f;
    private const float MaxValue = 100f;
    private const float MaxAlpha = 255f;

    /// <summary>
    /// Creates a color from HSV values using 0-359 hue, 0-100 saturation/value, and 0-255 alpha.
    /// </summary>
    public static Color FromHSV(int hue, int saturation = 100, int value = 100, int alpha = 255)
    {
        return Color.FromHsv(hue / MaxHue, saturation / MaxSaturation, value / MaxValue, alpha / MaxAlpha);
    }

    /// <summary>
    /// Generates a random color.
    /// </summary>
    public static Color Random(int alpha = 255)
    {
        float r = MathUtils.RandomRange(0.0, 1.0);
        float g = MathUtils.RandomRange(0.0, 1.0);
        float b = MathUtils.RandomRange(0.0, 1.0);

        return new Color(r, g, b, alpha / MaxAlpha);
    }
}
