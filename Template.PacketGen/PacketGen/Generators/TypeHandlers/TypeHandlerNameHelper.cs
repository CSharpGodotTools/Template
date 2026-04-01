using System.Text;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Helper for generating unique variable names during code generation.
/// </summary>
internal static class TypeHandlerNameHelper
{
    /// <summary>
    /// Builds a unique local variable name from a seed expression, a semantic role (e.g. "Count", "Index"),
    /// and a nesting depth to avoid collisions in nested loops.
    /// </summary>
    /// <param name="seed">Source text used as name seed.</param>
    /// <param name="role">Semantic suffix (for example Count or Index).</param>
    /// <param name="depth">Nesting depth used to avoid collisions.</param>
    /// <returns>Generated variable name.</returns>
    public static string BuildName(string seed, string role, int depth)
    {
        string normalizedSeed = Normalize(seed);
        return $"{normalizedSeed}{role}{depth}";
    }

    /// <summary>
    /// Strips non-alphanumeric characters, ensures the result starts with a letter or underscore,
    /// and lower-cases the first character to produce a valid camelCase identifier.
    /// </summary>
    /// <param name="seed">Source text used as name seed.</param>
    /// <returns>Normalized identifier base.</returns>
    private static string Normalize(string seed)
    {
        StringBuilder builder = new(seed.Length);

        foreach (char c in seed)
        {
            // Keep only identifier-safe alphanumeric characters.
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
        }

        // Fall back to a generic seed when normalization removed all characters.
        if (builder.Length == 0)
        {
            return "value";
        }

        string normalized = builder.ToString();
        char first = normalized[0];

        // Prefix with a letter when the identifier would otherwise start invalid.
        if (!char.IsLetter(first) && first != '_')
        {
            normalized = "v" + normalized;
        }

        return char.ToLowerInvariant(normalized[0]) + normalized.Substring(1);
    }
}
