#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static class VisualAnchorSettings
{
    /// <summary>
    /// Normalized anchor where (0,0)=top-left and (1,1)=bottom-right.
    /// </summary>
    public static Vector2 NormalizedAnchor { get; set; } = new(0.5f, 0.5f);
}
#endif
