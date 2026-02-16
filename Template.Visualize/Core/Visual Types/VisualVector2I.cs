#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector2I(VisualControlContext context)
    {
        return CreateVectorControl<Vector2I>(
            context,
            ["X", "Y"],
            typeof(int),
            value => [value.X, value.Y],
            (value, index, component) =>
            {
                if (index == 0)
                {
                    value.X = (int)component;
                }
                else
                {
                    value.Y = (int)component;
                }

                return value;
            });
    }
}
#endif
