#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Vector2I visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for <see cref="Vector2I"/> values.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created vector-control info.</returns>
    private static VisualControlInfo VisualVector2I(VisualControlContext context)
    {
        return CreateVectorControl<Vector2I>(
            context,
            ["X", "Y"],
            typeof(int),
            value => [value.X, value.Y],
            (value, index, component) =>
            {
                // Map component indices to vector axes.
                switch (index)
                {
                    case 0:
                        value.X = (int)component;
                        break;
                    default:
                        value.Y = (int)component;
                        break;
                }

                return value;
            });
    }
}
#endif
