#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static class VisualText
{
    public static string ToDisplayName(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return string.Empty;
        }

        return identifier.ToPascalCase().AddSpaceBeforeEachCapital();
    }

    public static string ToSpacedName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.AddSpaceBeforeEachCapital();
    }
}
#endif
