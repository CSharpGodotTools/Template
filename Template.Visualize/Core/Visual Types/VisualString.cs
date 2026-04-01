#if DEBUG
namespace GodotUtils.Debugging;

/// <summary>
/// String visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a text control for string values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created string-control info.</returns>
    private static VisualControlInfo VisualString(VisualControlContext context)
    {
        return CreateTextControl(
            context,
            text => text,
            value => value as string ?? string.Empty);
    }
}
#endif
