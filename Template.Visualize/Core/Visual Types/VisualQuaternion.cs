#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Quaternion visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for <see cref="Quaternion"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created vector-control info.</returns>
    private static VisualControlInfo VisualQuaternion(VisualControlContext context)
    {
        return CreateVectorControl<Quaternion>(
            context,
            ["X", "Y", "Z", "W"],
            typeof(float),
            value => [value.X, value.Y, value.Z, value.W],
            (value, index, component) =>
            {
                // Map component indices to quaternion components.
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
