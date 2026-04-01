#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Vector4I visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for <see cref="Vector4I"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created vector-control info.</returns>
    private static VisualControlInfo VisualVector4I(VisualControlContext context)
    {
        return CreateVectorControl<Vector4I>(
            context,
            ["X", "Y", "Z", "W"],
            typeof(int),
            value => [value.X, value.Y, value.Z, value.W],
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
