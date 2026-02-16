namespace GodotUtils;

public static class DoubleExtensions
{
    /// <summary>
    /// Returns true when the value has no fractional part.
    /// </summary>
    public static bool IsInteger(this double value)
    {
        return (value % 1) == 0;
    }
}
