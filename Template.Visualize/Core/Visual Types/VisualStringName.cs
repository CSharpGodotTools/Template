#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualStringName(VisualControlContext context)
    {
        return CreateTextControl(
            context,
            text => new StringName(text),
            value => value is StringName name ? name.ToString() : string.Empty);
    }
}
#endif
