using Godot;

namespace GodotUtils;

public static class InputUtils
{
    /// <summary>
    /// Returns a normalized movement vector from arrow or WASD input.
    /// </summary>
    public static Vector2 GetMoveVector()
    {
        float x = 0f;
        float y = 0f;

        if (Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A))
            x -= 1f;

        if (Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D))
            x += 1f;

        if (Input.IsKeyPressed(Key.Up) || Input.IsKeyPressed(Key.W))
            y -= 1f;

        if (Input.IsKeyPressed(Key.Down) || Input.IsKeyPressed(Key.S))
            y += 1f;

        Vector2 vector = new(x, y);
        return vector.LengthSquared() > 1f ? vector.Normalized() : vector;
    }
}
