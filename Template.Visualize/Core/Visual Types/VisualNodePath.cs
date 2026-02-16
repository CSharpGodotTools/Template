#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualNodePath(VisualControlContext context)
    {
        return CreateTextControl(
            context,
            text => new NodePath(text),
            value => value is NodePath path ? path.ToString() : string.Empty);
    }
}
#endif
