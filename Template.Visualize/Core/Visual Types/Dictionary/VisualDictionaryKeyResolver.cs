#if DEBUG
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Resolves duplicate keys by probing for valid alternatives.
/// </summary>
internal sealed class VisualDictionaryKeyResolver : IVisualDictionaryKeyResolver
{
    /// <summary>
    /// Attempts to resolve a renamed key collision.
    /// </summary>
    /// <param name="currentKey">Current key value.</param>
    /// <param name="duplicateKey">Incoming key that collides.</param>
    /// <param name="keyType">Expected key type.</param>
    /// <param name="containsKey">Key existence predicate.</param>
    /// <param name="resolvedKey">Resolved key when successful.</param>
    /// <returns><see langword="true"/> when a replacement key is found.</returns>
    public bool TryResolveRenamedKey(object currentKey, object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey)
    {
        resolvedKey = duplicateKey;

        // Require numeric conversion for numeric-based key resolution.
        if (!TryConvertToInt64(currentKey, out long currentNumeric)
            || !TryConvertToInt64(duplicateKey, out long duplicateNumeric))
        {
            return false;
        }

        long step = duplicateNumeric > currentNumeric ? 1 : -1;

        // Walk enum values to find the next free candidate.
        if (keyType.IsEnum)
        {
            long candidate = duplicateNumeric;

            do
            {
                candidate += step;
                resolvedKey = Enum.ToObject(keyType, candidate);
            }
            while (containsKey(resolvedKey));

            return true;
        }

        // Non-numeric types cannot be auto-adjusted here.
        if (!keyType.IsNumericType())
            return false;

        long numericCandidate = duplicateNumeric;

        // Walk numeric values until a free key is found.
        while (true)
        {
            numericCandidate += step;

            // Stop when conversion to the target type fails.
            if (!TryConvertFromInt64(numericCandidate, keyType, out object nextKey))
                return false;

            // Skip values already in use.
            if (containsKey(nextKey))
                continue;

            resolvedKey = nextKey;
            return true;
        }
    }

    /// <summary>
    /// Attempts to resolve a newly added key collision.
    /// </summary>
    /// <param name="duplicateKey">Incoming key that collides.</param>
    /// <param name="keyType">Expected key type.</param>
    /// <param name="containsKey">Key existence predicate.</param>
    /// <param name="resolvedKey">Resolved key when successful.</param>
    /// <returns><see langword="true"/> when a replacement key is found.</returns>
    public bool TryResolveAddedKey(object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey)
    {
        resolvedKey = duplicateKey;

        // Enum keys increment to the next available value.
        if (keyType.IsEnum)
        {
            // Bail when the enum key cannot be converted.
            if (!TryConvertToInt64(duplicateKey, out long candidate))
                return false;

            while (true)
            {
                candidate++;
                object enumKey = Enum.ToObject(keyType, candidate);

                // Skip enum candidates that are already present in the dictionary.
                if (containsKey(enumKey))
                    continue;

                resolvedKey = enumKey;
                return true;
            }
        }

        // Numeric keys increment to the next available value.
        if (keyType.IsNumericType())
        {
            // Bail when the numeric key cannot be converted.
            if (!TryConvertToInt64(duplicateKey, out long candidate))
                return false;

            while (true)
            {
                candidate++;

                // Abort if the incremented value cannot be represented by keyType.
                if (!TryConvertFromInt64(candidate, keyType, out object nextKey))
                    return false;

                // Skip values already in use.
                if (containsKey(nextKey))
                    continue;

                resolvedKey = nextKey;
                return true;
            }
        }

        // String/object keys get a numeric suffix appended.
        if (keyType == typeof(string) || keyType == typeof(object))
        {
            string baseKey = duplicateKey?.ToString() ?? "Key";
            int suffix = 1;

            while (true)
            {
                object nextKey = $"{baseKey} {suffix}";

                // Skip generated keys that are already in use.
                if (containsKey(nextKey))
                {
                    suffix++;
                    continue;
                }

                resolvedKey = nextKey;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to convert a value to <see cref="long"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <param name="convertedValue">Converted numeric value.</param>
    /// <returns><see langword="true"/> when conversion succeeds.</returns>
    private static bool TryConvertToInt64(object value, out long convertedValue)
    {
        try
        {
            convertedValue = Convert.ToInt64(value);
            return true;
        }
        catch (FormatException)
        {
        }
        catch (InvalidCastException)
        {
        }
        catch (OverflowException)
        {
        }

        convertedValue = 0;
        return false;
    }

    /// <summary>
    /// Attempts to convert a <see cref="long"/> to the target type.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <param name="targetType">Target type to convert to.</param>
    /// <param name="convertedValue">Converted value.</param>
    /// <returns><see langword="true"/> when conversion succeeds.</returns>
    private static bool TryConvertFromInt64(long value, Type targetType, out object convertedValue)
    {
        try
        {
            convertedValue = Convert.ChangeType(value, targetType)!;
            return true;
        }
        catch (FormatException)
        {
        }
        catch (InvalidCastException)
        {
        }
        catch (OverflowException)
        {
        }

        convertedValue = default!;
        return false;
    }
}
#endif
