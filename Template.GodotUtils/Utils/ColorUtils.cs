using Godot;

namespace GodotUtils;

/// <summary>
/// Color helper functions.
/// </summary>
public class ColorUtils
{
    /// <summary>
    /// Creates a color from HSV values using 0-359 hue, 0-100 saturation/value, and 0-255 alpha.
    /// </summary>
    public static Color FromHSV(int hue, int saturation = 100, int value = 100, int alpha = 255)
    {
        return Color.FromHsv(hue / 359f, saturation / 100f, value / 100f, alpha / 255f);
    }

    /// <summary>
    /// Generates a random color.
    /// </summary>
    public static Color Random(int alpha = 255)
    {
        float r = MathUtils.RandomRange(0.0, 1.0);
        float g = MathUtils.RandomRange(0.0, 1.0);
        float b = MathUtils.RandomRange(0.0, 1.0);

        return new Color(r, g, b, alpha / 255f);
    }
}
