using System.Text;

namespace __TEMPLATE__.Ui;

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
    /// <param name="label">Raw label or key text.</param>
    /// <returns>Normalized PascalCase key.</returns>
    public static string ToPascalCase(string label)
    {
        string source = label ?? string.Empty;

        // Empty or whitespace labels normalize to an empty persistence key.
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
            // Treat non-alphanumeric characters as word separators.
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

    /// <summary>
    /// Determines whether text contains non-alphanumeric separator characters.
    /// </summary>
    /// <param name="text">Input text to inspect.</param>
    /// <returns><see langword="true"/> when any separator exists.</returns>
    private static bool ContainsSeparator(string text)
    {
        foreach (char c in text)
        {
            // A non-alphanumeric character indicates separator-based tokenization.
            if (!char.IsLetterOrDigit(c))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Handles a single-word label with no separators.
    /// All-uppercase strings are lowered then title-cased.
    /// </summary>
    /// <param name="word">Single token to normalize.</param>
    /// <returns>Normalized PascalCase token.</returns>
    private static string NormaliseSingleWord(string word)
    {
        bool allUpper = true;

        foreach (char c in word)
        {
            // Detect mixed-case words so existing casing is preserved.
            if (char.IsLetter(c) && !char.IsUpper(c))
            {
                allUpper = false;
                break;
            }
        }

        // Acronyms are lowered first so PascalCase output remains readable.
        if (allUpper)
        {
            string lowered = word.ToLowerInvariant();
            return char.ToUpperInvariant(lowered[0]) + lowered[1..];
        }

        return char.ToUpperInvariant(word[0]) + word[1..];
    }
}
