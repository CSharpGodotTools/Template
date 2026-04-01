#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// String helpers for converting code identifiers to UI-friendly display text.
/// </summary>
internal static class VisualText
{
    /// <summary>
    /// Converts an identifier to spaced title text using PascalCase normalization.
    /// </summary>
    /// <param name="identifier">Source identifier.</param>
    /// <returns>Display-friendly text.</returns>
    public static string ToDisplayName(string identifier)
    {
        // Treat empty/whitespace input as no display text.
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return string.Empty;
        }

        return identifier.ToPascalCase().AddSpaceBeforeEachCapital();
    }

    /// <summary>
    /// Inserts spaces before capitals without forcing PascalCase first.
    /// </summary>
    /// <param name="value">Source text.</param>
    /// <returns>Spaced display text.</returns>
    public static string ToSpacedName(string value)
    {
        // Treat empty/whitespace input as no display text.
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.AddSpaceBeforeEachCapital();
    }
}
#endif
