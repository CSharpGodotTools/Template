#if DEBUG
namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualObject(VisualControlContext context)
    {
        return CreateTextControl(
            context,
            text => text,
            value => value?.ToString() ?? string.Empty);
    }
}
#endif
