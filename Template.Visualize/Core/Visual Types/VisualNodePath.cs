#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// NodePath visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a text control for <see cref="NodePath"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created node-path control info.</returns>
    private static VisualControlInfo VisualNodePath(VisualControlContext context)
    {
        // Convert between NodePath values and their string representation.
        return CreateTextControl(
            context,
            text => new NodePath(text),
            value => value is NodePath path ? path.ToString() : string.Empty);
    }
}
#endif
