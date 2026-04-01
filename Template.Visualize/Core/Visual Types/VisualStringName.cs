#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// StringName visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a text control for <see cref="StringName"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created string-name control info.</returns>
    private static VisualControlInfo VisualStringName(VisualControlContext context)
    {
        // Convert between StringName values and their string representation.
        return CreateTextControl(
            context,
            text => new StringName(text),
            value => value is StringName name ? name.ToString() : string.Empty);
    }
}
#endif
