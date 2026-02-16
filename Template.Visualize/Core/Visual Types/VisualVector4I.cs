#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector4I(VisualControlContext context)
    {
        return CreateVectorControl<Vector4I>(
            context,
            ["X", "Y", "Z", "W"],
            typeof(int),
            value => [value.X, value.Y, value.Z, value.W],
            (value, index, component) =>
            {
                switch (index)
                {
                    case 0:
                        value.X = (int)component;
                        break;
                    case 1:
                        value.Y = (int)component;
                        break;
                    case 2:
                        value.Z = (int)component;
                        break;
                    default:
                        value.W = (int)component;
                        break;
                }

                return value;
            });
    }
}
#endif
