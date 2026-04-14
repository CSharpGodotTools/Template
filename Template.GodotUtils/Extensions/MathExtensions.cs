using Godot;
using System;

namespace GodotUtils;

/// <summary>
/// Numeric and geometry extension helpers used across gameplay code.
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    /// <param name="color1">Starting color value.</param>
    /// <param name="color2">Ending color value.</param>
    /// <param name="t">Interpolation factor in the range 0..1.</param>
    /// <returns>Interpolated color value.</returns>
    public static Color Lerp(this Color color1, Color color2, float t)
    {
        return (color1 * (1 - t)) + (color2 * t);
    }

    /// <summary>
    /// Remaps a value from one range to another.
    /// </summary>
    /// <param name="value">Input value in the source range.</param>
    /// <param name="from1">Source range minimum.</param>
    /// <param name="to1">Source range maximum.</param>
    /// <param name="from2">Destination range minimum.</param>
    /// <param name="to2">Destination range maximum.</param>
    /// <returns>Input value mapped into the destination range.</returns>
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return ((value - from1) / (to1 - from1) * (to2 - from2)) + from2;
    }

    /// <summary>
    /// Smoothly rotates a sprite toward a target position.
    /// </summary>
    /// <param name="sprite">Sprite to rotate.</param>
    /// <param name="target">World position to face.</param>
    /// <param name="t">Smoothing factor used by angle interpolation.</param>
    public static void LerpRotationToTarget(this Sprite2D sprite, Vector2 target, float t = 0.1f)
    {
        sprite.Rotation = Mathf.LerpAngle(sprite.Rotation, (target - sprite.GlobalPosition).Angle(), t);
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">Angle in degrees.</param>
    /// <returns>Equivalent angle in radians.</returns>
    public static float ToRadians(this float degrees)
    {
        return degrees * (Mathf.Pi / 180);
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="radians">Angle in radians.</param>
    /// <returns>Equivalent angle in degrees.</returns>
    public static float ToDegrees(this float radians)
    {
        return radians * (180 / Mathf.Pi);
    }

    /// <summary>
    /// Clamps an integer between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="v">Input value to clamp.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound.</param>
    /// <returns>Clamped integer value.</returns>
    public static int Clamp(this int v, int min, int max)
    {
        return Mathf.Clamp(v, min, max);
    }

    /// <summary>
    /// Clamps a float between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="v">Input value to clamp.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound.</param>
    /// <returns>Clamped float value.</returns>
    public static float Clamp(this float v, float min, float max)
    {
        return Mathf.Clamp(v, min, max);
    }

    /// <summary>
    /// Linearly interpolates between two floats.
    /// </summary>
    /// <param name="a">Starting value.</param>
    /// <param name="b">Ending value.</param>
    /// <param name="t">Interpolation factor in the range 0..1.</param>
    /// <returns>Interpolated float value.</returns>
    public static float Lerp(this float a, float b, float t)
    {
        return Mathf.Lerp(a, b, t);
    }

    /// <summary>
    /// Pulses a value from 0 to 1 over time.
    /// </summary>
    /// <param name="time">Time value in seconds.</param>
    /// <param name="frequency">Pulse frequency multiplier.</param>
    /// <returns>Pulsing value in the range 0..1.</returns>
    public static float Pulse(this float time, float frequency)
    {
        return 0.5f * (1 + Mathf.Sin(2 * Mathf.Pi * frequency * time));
    }

    /// <summary>
    /// Counts the number of digits in the value.
    /// </summary>
    /// <param name="num">Input integer value.</param>
    /// <returns>Number of decimal digits in the absolute value.</returns>
    public static int CountDigits(this int num)
    {
        long value = Math.Abs((long)num);

        // Zero is treated as a single-digit value.
        if (value == 0)
            return 1;

        return (int)Math.Floor(Math.Log10(value) + 1);
    }

    /// <summary>
    /// Counts the number of digits in the value.
    /// </summary>
    /// <param name="num">Input unsigned short value.</param>
    /// <returns>Number of decimal digits.</returns>
    public static ushort CountDigits(this ushort num)
    {
        // Zero is treated as a single-digit value.
        if (num == 0)
            return 1;

        return (ushort)Math.Floor(Math.Log10(num) + 1);
    }
}
