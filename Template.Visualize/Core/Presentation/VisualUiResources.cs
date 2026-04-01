#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Shared color resources used by visualization UI sections.
/// </summary>
internal static class VisualUiResources
{
    /// <summary>
    /// Tint used for mutable-member columns.
    /// </summary>
    public static readonly Color MutableMembersColor = new(0.8f, 1, 0.8f);

    /// <summary>
    /// Tint used for readonly-member columns.
    /// </summary>
    public static readonly Color ReadonlyMembersColor = new(1.0f, 0.75f, 0.8f);
}
#endif
