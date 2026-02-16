#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector3(VisualControlContext context)
    {
        return CreateVectorControl<Vector3>(
            context,
            ["X", "Y", "Z"],
            typeof(float),
            value => [value.X, value.Y, value.Z],
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
                    default:
                        value.Z = (float)component;
                        break;
                }

                return value;
            });
    }
}
#endif
