#if DEBUG
using System;

namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryKeyResolver : IVisualDictionaryKeyResolver
{
    public bool TryResolveRenamedKey(object currentKey, object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey)
    {
        resolvedKey = duplicateKey;

        if (!TryConvertToInt64(currentKey, out long currentNumeric)
            || !TryConvertToInt64(duplicateKey, out long duplicateNumeric))
        {
            return false;
        }

        long step = duplicateNumeric > currentNumeric ? 1 : -1;

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

        if (!keyType.IsNumericType())
        {
            return false;
        }

        long numericCandidate = duplicateNumeric;

        while (true)
        {
            numericCandidate += step;

            if (!TryConvertFromInt64(numericCandidate, keyType, out object nextKey))
            {
                return false;
            }

            if (containsKey(nextKey))
            {
                continue;
            }

            resolvedKey = nextKey;
            return true;
        }
    }

    public bool TryResolveAddedKey(object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey)
    {
        resolvedKey = duplicateKey;

        if (keyType.IsEnum)
        {
            if (!TryConvertToInt64(duplicateKey, out long candidate))
            {
                return false;
            }

            while (true)
            {
                candidate += 1;
                object enumKey = Enum.ToObject(keyType, candidate);

                if (containsKey(enumKey))
                {
                    continue;
                }

                resolvedKey = enumKey;
                return true;
            }
        }

        if (keyType.IsNumericType())
        {
            if (!TryConvertToInt64(duplicateKey, out long candidate))
            {
                return false;
            }

            while (true)
            {
                candidate += 1;

                if (!TryConvertFromInt64(candidate, keyType, out object nextKey))
                {
                    return false;
                }

                if (containsKey(nextKey))
                {
                    continue;
                }

                resolvedKey = nextKey;
                return true;
            }
        }

        if (keyType == typeof(string) || keyType == typeof(object))
        {
            string baseKey = duplicateKey?.ToString() ?? "Key";
            int suffix = 1;

            while (true)
            {
                object nextKey = $"{baseKey} {suffix}";

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
