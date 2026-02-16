using Godot;

namespace GodotUtils;

public static class VectorUtils
{
    /// <summary>
    /// Returns a random normalized 2D direction vector.
    /// </summary>
    public static Vector2 RandomDirection2D()
    {
        Vector2 vector = new(MathUtils.RandomRange(-1.0, 1.0), MathUtils.RandomRange(-1.0, 1.0));
        return vector.Normalized();
    }
}
