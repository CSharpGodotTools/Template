#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal interface IVisualTitleBarValueSyncService
{
    void SyncMutableFromReadonly(Control mutableMembersVbox, Control readonlyMembersVbox);
}
#endif
