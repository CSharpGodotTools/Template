using Godot;

namespace GodotUtils;

/// <summary>
/// Helper methods for common vector generation tasks.
/// </summary>
public static class VectorUtils
{
    /// <summary>
    /// Returns a random normalized 2D direction vector.
    /// </summary>
    /// <returns>Random unit-length 2D direction.</returns>
    public static Vector2 RandomDirection2D()
    {
        Vector2 vector = new(MathUtils.RandomRange(-1.0, 1.0), MathUtils.RandomRange(-1.0, 1.0));
        return vector.Normalized();
    }
}
