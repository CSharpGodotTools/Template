#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Synchronizes editable values with readonly snapshots in visual title-bar panels.
/// </summary>
internal interface IVisualTitleBarValueSyncService
{
    /// <summary>
    /// Copies readonly column values into corresponding mutable controls.
    /// </summary>
    /// <param name="mutableMembersVbox">Container with mutable member rows.</param>
    /// <param name="readonlyMembersVbox">Container with readonly member rows.</param>
    void SyncMutableFromReadonly(Control mutableMembersVbox, Control readonlyMembersVbox);
}
#endif
