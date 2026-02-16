using Godot;

namespace GodotUtils;

public static class MathUtils
{
    /// <summary>
    /// <para>Returns the sum of the first n natural numbers</para>
    /// <para>For example if n = 4 then this would return 0 + 1 + 2 + 3</para>
    /// </summary>
    public static int SumNaturalNumbers(int n)
    {
        return (n * (n - 1)) / 2;
    }

    /// <summary>
    /// Computes <paramref name="x"/> raised to <paramref name="pow"/>.
    /// </summary>
    public static uint UIntPow(this uint x, uint pow)
    {
        uint ret = 1;

        // Fast power using exponentiation by squaring.
        while (pow != 0)
        {
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
    public static float RandomRange(double min, double max)
    {
        return (float)GD.RandRange(min, max);
    }

    /// <summary>
    /// Returns a random integer within the range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    public static int RandomRangeInt(int min, int max)
    {
        return GD.RandRange(min, max);
    }

    /// <summary>
    /// Returns a random unit direction vector.
    /// </summary>
    public static Vector2 RandDir()
    {
        float theta = RandAngle();
        return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
    }

    /// <summary>
    /// Returns a random angle in radians between 0 and 2π.
    /// </summary>
    public static float RandAngle()
    {
        return RandomRange(0, Mathf.Pi * 2);
    }

    /// <summary>
    /// Returns the angle from <paramref name="from"/> to <paramref name="to"/> in radians.
    /// </summary>
    public static float GetAngle(Vector2 to, Vector2 from)
    {
        return (to - from).Angle();
    }

    /// <summary>
    /// Returns the wrapped angular difference between <paramref name="to"/> and <paramref name="from"/>.
    /// </summary>
    public static float GetAngleDiff(float to, float from)
    {
        return Mathf.Wrap(from - to, -Mathf.Pi, Mathf.Pi);
    }
}
