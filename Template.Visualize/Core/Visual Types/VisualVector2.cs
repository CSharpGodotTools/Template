#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector2(VisualControlContext context)
    {
        return CreateVectorControl<Vector2>(
            context,
            ["X", "Y"],
            typeof(float),
            value => [value.X, value.Y],
            (value, index, component) =>
            {
                switch (index)
                {
                    case 0:
                        value.X = (float)component;
                        break;
                    default:
                        value.Y = (float)component;
                        break;
                }

                return value;
            });
    }
}
#endif
