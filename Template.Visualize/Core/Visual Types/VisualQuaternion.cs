#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualQuaternion(VisualControlContext context)
    {
        return CreateVectorControl<Quaternion>(
            context,
            ["X", "Y", "Z", "W"],
            typeof(float),
            value => [value.X, value.Y, value.Z, value.W],
            (value, index, component) =>
            {
                switch (index)
                {
                    case 0:
                        value.X = (float)component;
                        break;
                    case 1:
                        value.Y = (float)component;
                        break;
                    case 2:
                        value.Z = (float)component;
                        break;
                    default:
                        value.W = (float)component;
                        break;
                }

                return value;
            });
    }
}
#endif
