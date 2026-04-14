using Godot;

namespace GodotUtils;

/// <summary>
/// Input helper methods for common movement and key-state patterns.
/// </summary>
public static class InputUtils
{
    /// <summary>
    /// Returns a normalized movement vector from arrow or WASD input.
    /// </summary>
    /// <returns>Normalized movement vector based on active directional keys.</returns>
    public static Vector2 GetMoveVector()
    {
        float x = 0f;
        float y = 0f;

        // Apply negative X movement for left input.
        if (Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A))
            x--;

        // Apply positive X movement for right input.
        if (Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D))
            x++;

        // Apply negative Y movement for up input.
        if (Input.IsKeyPressed(Key.Up) || Input.IsKeyPressed(Key.W))
            y--;

        // Apply positive Y movement for down input.
        if (Input.IsKeyPressed(Key.Down) || Input.IsKeyPressed(Key.S))
            y++;

        Vector2 vector = new(x, y);
        return vector.LengthSquared() > 1f ? vector.Normalized() : vector;
    }
}
