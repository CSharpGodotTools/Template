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
    /// <param name="camera">Camera to reposition.</param>
    /// <param name="position">Target position.</param>
    public static void SetPositionIgnoreSmoothing(this Camera2D camera, Vector2 position)
    {
        bool smoothEnabled = camera.PositionSmoothingEnabled;

        // Temporarily disable smoothing for a snap move.
        if (smoothEnabled)
        {
            // Re-enable smoothing only when it was previously enabled.
            camera.PositionSmoothingEnabled = false;
        }

        camera.Position = position;

        // Restore smoothing only when it was active before the snap operation.
        if (smoothEnabled)
        {
            // Restore smoothing after the snapped position has been applied.
            Tweens.Delay(camera, 0.01, () => camera.PositionSmoothingEnabled = true);
        }
    }
}
