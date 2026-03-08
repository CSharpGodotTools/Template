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
    public static string BuildName(string seed, string role, int depth)
    {
        string normalizedSeed = Normalize(seed);
        return $"{normalizedSeed}{role}{depth}";
    }

    /// <summary>
    /// Strips non-alphanumeric characters, ensures the result starts with a letter or underscore,
    /// and lower-cases the first character to produce a valid camelCase identifier.
    /// </summary>
    private static string Normalize(string seed)
    {
        StringBuilder builder = new(seed.Length);

        foreach (char c in seed)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
        }

        if (builder.Length == 0)
        {
            return "value";
        }

        string normalized = builder.ToString();
        char first = normalized[0];

        if (!char.IsLetter(first) && first != '_')
        {
            normalized = "v" + normalized;
        }

        return char.ToLowerInvariant(normalized[0]) + normalized.Substring(1);
    }
}
