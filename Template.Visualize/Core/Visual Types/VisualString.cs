#if DEBUG
namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualString(VisualControlContext context)
    {
        return CreateTextControl(
            context,
            text => text,
            value => value as string ?? string.Empty);
    }
}
#endif
