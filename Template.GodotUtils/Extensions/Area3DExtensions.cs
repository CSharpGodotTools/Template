using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for 3D areas.
/// </summary>
public static class Area3DExtensions
{
    /// <summary>
    /// Sets Monitoring using deferred property updates.
    /// </summary>
    /// <param name="area">Area to update.</param>
    /// <param name="enabled">Target monitoring state.</param>
    public static void SetMonitoringDeferred(this Area3D area, bool enabled)
    {
        area.SetDeferred(Area3D.PropertyName.Monitoring, enabled);
    }

    /// <summary>
    /// Sets Monitorable using deferred property updates.
    /// </summary>
    /// <param name="area">Area to update.</param>
    /// <param name="enabled">Target monitorable state.</param>
    public static void SetMonitorableDeferred(this Area3D area, bool enabled)
    {
        area.SetDeferred(Area3D.PropertyName.Monitorable, enabled);
    }
}
