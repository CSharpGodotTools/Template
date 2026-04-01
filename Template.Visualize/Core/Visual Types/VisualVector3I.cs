#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Vector3I visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for <see cref="Vector3I"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created vector-control info.</returns>
    private static VisualControlInfo VisualVector3I(VisualControlContext context)
    {
        return CreateVectorControl<Vector3I>(
            context,
            ["X", "Y", "Z"],
            typeof(int),
            value => [value.X, value.Y, value.Z],
            (value, index, component) =>
            {
                // Map component indices to vector axes.
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
