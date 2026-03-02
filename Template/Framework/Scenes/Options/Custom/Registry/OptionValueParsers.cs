using System.Text.Json;
using System.Globalization;

namespace Framework.UI;

/// <summary>
/// JSON element parsers used by <see cref="OptionPersistence"/> to
/// deserialise typed values from <see cref="ResourceOptions.CustomOptionValues"/>.
/// </summary>
internal static class OptionValueParsers
{
    internal static (bool, float) ParseFloat(JsonElement element, float defaultValue)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetSingle(out float f)) return (true, f);
            if (element.TryGetDouble(out double d)) return (true, (float)d);
        }

        if (element.ValueKind == JsonValueKind.String &&
            float.TryParse(element.GetString(), NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out float parsed))
            return (true, parsed);

        return (false, defaultValue);
    }

    internal static (bool, int) ParseInt(JsonElement element, int defaultValue)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetInt32(out int i)) return (true, i);
            if (element.TryGetDouble(out double d)) return (true, (int)d);
        }

        if (element.ValueKind == JsonValueKind.String &&
            int.TryParse(element.GetString(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int parsed))
            return (true, parsed);

        return (false, defaultValue);
    }

    internal static (bool, string) ParseString(JsonElement element, string defaultValue)
    {
        if (element.ValueKind == JsonValueKind.String)
            return (true, element.GetString()!);

        return (false, defaultValue);
    }

    internal static (bool, bool) ParseBool(JsonElement element, bool defaultValue)
    {
        if (element.ValueKind is JsonValueKind.True) return (true, true);
        if (element.ValueKind is JsonValueKind.False) return (true, false);

        if (element.ValueKind == JsonValueKind.String &&
            bool.TryParse(element.GetString(), out bool parsed))
            return (true, parsed);

        return (false, defaultValue);
    }
}
