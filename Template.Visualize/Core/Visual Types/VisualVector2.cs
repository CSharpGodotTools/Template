#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Vector2 visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for <see cref="Vector2"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created vector-control info.</returns>
    private static VisualControlInfo VisualVector2(VisualControlContext context)
    {
        return CreateVectorControl<Vector2>(
            context,
            ["X", "Y"],
            typeof(float),
            value => [value.X, value.Y],
            (value, index, component) =>
            {
                // Map component indices to vector axes.
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
