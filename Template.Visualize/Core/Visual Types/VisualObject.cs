#if DEBUG
namespace GodotUtils.Debugging;

/// <summary>
/// Object stringification visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a text control that uses <see cref="object.ToString"/> for display.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created text-control info.</returns>
    private static VisualControlInfo VisualObject(VisualControlContext context)
    {
        // Convert null values to an empty string for display.
        return CreateTextControl(
            context,
            text => text,
            value => value?.ToString() ?? string.Empty);
    }
}
#endif
