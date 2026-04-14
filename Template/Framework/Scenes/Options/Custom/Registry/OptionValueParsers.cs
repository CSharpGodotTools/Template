using System.Globalization;
using System.Text.Json;

namespace __TEMPLATE__.Ui;

/// <summary>
/// JSON element parsers used by <see cref="OptionPersistence"/> to
/// deserialise typed values from <see cref="ResourceOptions.CustomOptionValues"/>.
/// </summary>
internal static class OptionValueParsers
{
    /// <summary>
    /// Attempts to parse a float from a JSON element.
    /// </summary>
    /// <param name="element">JSON element to parse.</param>
    /// <param name="defaultValue">Fallback value when parsing fails.</param>
    /// <returns>Tuple indicating parse success and resulting value.</returns>
    internal static (bool, float) ParseFloat(JsonElement element, float defaultValue)
    {
        // Prefer numeric JSON tokens for float parsing.
        if (element.ValueKind == JsonValueKind.Number)
        {
            // Use exact single-precision value when available.
            if (element.TryGetSingle(out float f)) return (true, f);

            // Fall back to double conversion when single extraction fails.
            if (element.TryGetDouble(out double d)) return (true, (float)d);
        }

        // Accept invariant-culture numeric strings as a secondary source.
        if (element.ValueKind == JsonValueKind.String &&
            float.TryParse(element.GetString(), NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out float parsed))
        {
            return (true, parsed);
        }

        return (false, defaultValue);
    }

    /// <summary>
    /// Attempts to parse an int from a JSON element.
    /// </summary>
    /// <param name="element">JSON element to parse.</param>
    /// <param name="defaultValue">Fallback value when parsing fails.</param>
    /// <returns>Tuple indicating parse success and resulting value.</returns>
    internal static (bool, int) ParseInt(JsonElement element, int defaultValue)
    {
        // Prefer numeric JSON tokens for integer parsing.
        if (element.ValueKind == JsonValueKind.Number)
        {
            // Use direct int extraction when representable.
            if (element.TryGetInt32(out int i)) return (true, i);

            // Fall back to double conversion when int extraction fails.
            if (element.TryGetDouble(out double d)) return (true, (int)d);
        }

        // Accept invariant-culture integer strings as a secondary source.
        if (element.ValueKind == JsonValueKind.String &&
            int.TryParse(element.GetString(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int parsed))
        {
            return (true, parsed);
        }

        return (false, defaultValue);
    }

    /// <summary>
    /// Attempts to parse a string from a JSON element.
    /// </summary>
    /// <param name="element">JSON element to parse.</param>
    /// <param name="defaultValue">Fallback value when parsing fails.</param>
    /// <returns>Tuple indicating parse success and resulting value.</returns>
    internal static (bool, string) ParseString(JsonElement element, string defaultValue)
    {
        // Accept only JSON string tokens for string option values.
        if (element.ValueKind == JsonValueKind.String)
            return (true, element.GetString()!);

        return (false, defaultValue);
    }

    /// <summary>
    /// Attempts to parse a boolean from a JSON element.
    /// </summary>
    /// <param name="element">JSON element to parse.</param>
    /// <param name="defaultValue">Fallback value when parsing fails.</param>
    /// <returns>Tuple indicating parse success and resulting value.</returns>
    internal static (bool, bool) ParseBool(JsonElement element, bool defaultValue)
    {
        // Accept native JSON true token.
        if (element.ValueKind is JsonValueKind.True) return (true, true);

        // Accept native JSON false token.
        if (element.ValueKind is JsonValueKind.False) return (true, false);

        // Accept boolean text values as a compatibility fallback.
        if (element.ValueKind == JsonValueKind.String &&
            bool.TryParse(element.GetString(), out bool parsed))
        {
            return (true, parsed);
        }

        return (false, defaultValue);
    }
}
