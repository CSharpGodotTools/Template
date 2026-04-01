namespace GodotUtils;

/// <summary>
/// Extension helpers for <see cref="double"/> values.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Returns true when the value has no fractional part.
    /// </summary>
    /// <param name="value">Value to test.</param>
    /// <returns><see langword="true"/> when value is mathematically an integer.</returns>
    public static bool IsInteger(this double value)
    {
        return (value % 1) == 0;
    }
}
