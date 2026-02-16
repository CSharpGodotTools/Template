using System.Text;

namespace PacketGen.Generators.TypeHandlers;

internal static class TypeHandlerNameHelper
{
    public static string BuildName(string seed, string role, int depth)
    {
        string normalizedSeed = Normalize(seed);
        return $"{normalizedSeed}{role}{depth}";
    }

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
