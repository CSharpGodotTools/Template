using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for 2D cameras.
/// </summary>
public static class Camera2DExtensions
{
    /// <summary>
    /// Sets the camera position without smoothing, then restores smoothing if needed.
    /// </summary>
    public static void SetPositionIgnoreSmoothing(this Camera2D camera, Vector2 position)
    {
        bool smoothEnabled = camera.PositionSmoothingEnabled;

        // Temporarily disable smoothing for a snap move.
        if (smoothEnabled)
        {
            camera.PositionSmoothingEnabled = false;
        }

        camera.Position = position;

        if (smoothEnabled)
        {
            Tweens.Delay(camera, 0.01, () => camera.PositionSmoothingEnabled = true);
        }
    }
}
