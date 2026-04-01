#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Vector4 visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for <see cref="Vector4"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created vector-control info.</returns>
    private static VisualControlInfo VisualVector4(VisualControlContext context)
    {
        return CreateVectorControl<Vector4>(
            context,
            ["X", "Y", "Z", "W"],
            typeof(float),
            value => [value.X, value.Y, value.Z, value.W],
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
