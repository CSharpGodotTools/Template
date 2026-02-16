#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector3I(VisualControlContext context)
    {
        return CreateVectorControl<Vector3I>(
            context,
            ["X", "Y", "Z"],
            typeof(int),
            value => [value.X, value.Y, value.Z],
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
                    default:
                        value.Z = (int)component;
                        break;
                }

                return value;
            });
    }
}
#endif
