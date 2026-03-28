#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static class VisualUiResources
{
    public static readonly Color MutableMembersColor = new(0.8f, 1, 0.8f);
    public static readonly Color ReadonlyMembersColor = new(1.0f, 0.75f, 0.8f);
}
#endif
