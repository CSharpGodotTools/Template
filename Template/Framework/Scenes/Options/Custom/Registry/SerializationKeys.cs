using System.Text;

namespace Framework.UI;

/// <summary>
/// Converts option labels into PascalCase keys suitable for JSON persistence.
/// </summary>
internal static class SerializationKeys
{
    /// <summary>
    /// Transforms <paramref name="label"/> into a PascalCase string
    /// by splitting on non-alphanumerics and capitalising each word.
    /// All-uppercase tokens are lowered first so "FOV" becomes "Fov".
    /// </summary>
    public static string ToPascalCase(string label)
    {
        string source = label ?? string.Empty;

        if (string.IsNullOrWhiteSpace(source))
            return string.Empty;

        // Fast path: no separators present
        if (!ContainsSeparator(source))
            return NormaliseSingleWord(source);

        // Multi-word: split on non-alphanumeric characters
        StringBuilder result = new();
        bool capitaliseNext = true;

        foreach (char c in source)
        {
            if (!char.IsLetterOrDigit(c))
            {
                capitaliseNext = true;
                continue;
            }

            result.Append(capitaliseNext ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
            capitaliseNext = false;
        }

        return result.ToString();
    }

    private static bool ContainsSeparator(string text)
    {
        foreach (char c in text)
        {
            if (!char.IsLetterOrDigit(c))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Handles a single-word label with no separators.
    /// All-uppercase strings are lowered then title-cased.
    /// </summary>
    private static string NormaliseSingleWord(string word)
    {
        bool allUpper = true;

        foreach (char c in word)
        {
            if (char.IsLetter(c) && !char.IsUpper(c))
            {
                allUpper = false;
                break;
            }
        }

        if (allUpper)
        {
            string lowered = word.ToLowerInvariant();
            return char.ToUpperInvariant(lowered[0]) + lowered[1..];
        }

        return char.ToUpperInvariant(word[0]) + word[1..];
    }
}
