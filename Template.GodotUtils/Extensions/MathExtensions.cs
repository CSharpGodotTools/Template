using Godot;
using System;

namespace GodotUtils;

/// <summary>
/// Extension helpers for math utilities.
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    public static Color Lerp(this Color color1, Color color2, float t)
    {
        return color1 * (1 - t) + color2 * t;
    }

    /// <summary>
    /// Remaps a value from one range to another.
    /// </summary>
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    /// <summary>
    /// Smoothly rotates a sprite toward a target position.
    /// </summary>
    public static void LerpRotationToTarget(this Sprite2D sprite, Vector2 target, float t = 0.1f)
    {
        sprite.Rotation = Mathf.LerpAngle(sprite.Rotation, (target - sprite.GlobalPosition).Angle(), t);
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    public static float ToRadians(this float degrees)
    {
        return degrees * (Mathf.Pi / 180);
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    public static float ToDegrees(this float radians)
    {
        return radians * (180 / Mathf.Pi);
    }

    /// <summary>
    /// Clamps an integer between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    public static int Clamp(this int v, int min, int max)
    {
        return Mathf.Clamp(v, min, max);
    }

    /// <summary>
    /// Clamps a float between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    public static float Clamp(this float v, float min, float max)
    {
        return Mathf.Clamp(v, min, max);
    }

    /// <summary>
    /// Linearly interpolates between two floats.
    /// </summary>
    public static float Lerp(this float a, float b, float t)
    {
        return Mathf.Lerp(a, b, t);
    }

    /// <summary>
    /// Pulses a value from 0 to 1 over time.
    /// </summary>
    public static float Pulse(this float time, float frequency)
    {
        return 0.5f * (1 + Mathf.Sin(2 * Mathf.Pi * frequency * time));
    }

    /// <summary>
    /// Counts the number of digits in the value.
    /// </summary>
    public static int CountDigits(this int num)
    {
        long value = Math.Abs((long)num);
        if (value == 0)
            return 1;

        return (int)Math.Floor(Math.Log10(value) + 1);
    }

    /// <summary>
    /// Counts the number of digits in the value.
    /// </summary>
    public static ushort CountDigits(this ushort num)
    {
        if (num == 0)
            return 1;

        return (ushort)Math.Floor(Math.Log10(num) + 1);
    }
}
