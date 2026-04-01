#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Vector3 visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for <see cref="Vector3"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created vector-control info.</returns>
    private static VisualControlInfo VisualVector3(VisualControlContext context)
    {
        return CreateVectorControl<Vector3>(
            context,
            ["X", "Y", "Z"],
            typeof(float),
            value => [value.X, value.Y, value.Z],
            (value, index, component) =>
            {
                // Map component indices to vector axes.
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
