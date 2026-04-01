using Godot;

namespace GodotUtils;

/// <summary>
/// Shared math helpers for randomization, bit-layer masks, and angle operations.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// <para>Returns the sum of the first n natural numbers</para>
    /// <para>For example if n = 4 then this would return 0 + 1 + 2 + 3</para>
    /// </summary>
    /// <param name="n">Number of natural numbers to sum.</param>
    /// <returns>Sum of natural numbers in the range [0, n).</returns>
    public static int SumNaturalNumbers(int n)
    {
        return (n * (n - 1)) / 2;
    }

    /// <summary>
    /// Computes <paramref name="x"/> raised to <paramref name="pow"/>.
    /// </summary>
    /// <param name="x">Base value.</param>
    /// <param name="pow">Exponent value.</param>
    /// <returns>Result of raising <paramref name="x"/> to <paramref name="pow"/>.</returns>
    public static uint UIntPow(this uint x, uint pow)
    {
        uint ret = 1;

        // Fast power using exponentiation by squaring.
        while (pow != 0)
        {
            // Multiply accumulator when current exponent bit is set.
            if ((pow & 1) == 1)
            {
                ret *= x;
            }

            x *= x;
            pow >>= 1;
        }

        return ret;
    }

    /// <summary>
    /// Godot has nodes with properties like LightMask, CollisionLayer
    /// and MaskLayer. All these properties are integers. Simply setting
    /// the property to a number like 5 won't enable the 5th layer. The
    /// number needs to be in binary and that is what this function is for.
    /// 
    /// <para>For example to enable the 1st, 4th and 5th layers of a players
    /// collision layer you would do the following.</para>
    /// 
    /// <code>player.CollisionLayer = MathUtils.GetLayerValues(1, 4, 5)</code>
    /// </summary>
    /// <param name="layers">One-based layer indices to enable in the resulting mask.</param>
    /// <returns>Bitmask value with the requested layers enabled.</returns>
    public static int GetLayerValues(params int[] layers)
    {
        int num = 0;

        foreach (int layer in layers)
        {
            num |= 1 << layer - 1;
        }

        return num;
    }

    /// <summary>
    /// Returns a random float within the range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <param name="min">Minimum inclusive random value.</param>
    /// <param name="max">Maximum inclusive random value.</param>
    /// <returns>Random float in the requested range.</returns>
    public static float RandomRange(double min, double max)
    {
        return (float)GD.RandRange(min, max);
    }

    /// <summary>
    /// Returns a random integer within the range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <param name="min">Minimum inclusive random value.</param>
    /// <param name="max">Maximum inclusive random value.</param>
    /// <returns>Random integer in the requested range.</returns>
    public static int RandomRangeInt(int min, int max)
    {
        return GD.RandRange(min, max);
    }

    /// <summary>
    /// Returns a random unit direction vector.
    /// </summary>
    /// <returns>Random normalized direction vector.</returns>
    public static Vector2 RandDir()
    {
        float theta = RandAngle();
        return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
    }

    /// <summary>
    /// Returns a random angle in radians between 0 and 2π.
    /// </summary>
    /// <returns>Random angle in radians.</returns>
    public static float RandAngle()
    {
        return RandomRange(0, Mathf.Pi * 2);
    }

    /// <summary>
    /// Returns the angle from <paramref name="from"/> to <paramref name="to"/> in radians.
    /// </summary>
    /// <param name="to">Target position.</param>
    /// <param name="from">Origin position.</param>
    /// <returns>Angle from origin to target in radians.</returns>
    public static float GetAngle(Vector2 to, Vector2 from)
    {
        return (to - from).Angle();
    }

    /// <summary>
    /// Returns the wrapped angular difference between <paramref name="to"/> and <paramref name="from"/>.
    /// </summary>
    /// <param name="to">Target angle in radians.</param>
    /// <param name="from">Source angle in radians.</param>
    /// <returns>Wrapped angular difference in the range [-π, π].</returns>
    public static float GetAngleDiff(float to, float from)
    {
        return Mathf.Wrap(from - to, -Mathf.Pi, Mathf.Pi);
    }
}
