using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for 2D areas.
/// </summary>
public static class Area2DExtensions
{
    /// <summary>
    /// Sets Monitoring using deferred property updates.
    /// </summary>
    /// <param name="area">Area to update.</param>
    /// <param name="enabled">Target monitoring state.</param>
    public static void SetMonitoringDeferred(this Area2D area, bool enabled)
    {
        area.SetDeferred(Area2D.PropertyName.Monitoring, enabled);
    }

    /// <summary>
    /// Sets Monitorable using deferred property updates.
    /// </summary>
    /// <param name="area">Area to update.</param>
    /// <param name="enabled">Target monitorable state.</param>
    public static void SetMonitorableDeferred(this Area2D area, bool enabled)
    {
        area.SetDeferred(Area2D.PropertyName.Monitorable, enabled);
    }
}
