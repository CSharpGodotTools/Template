#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Debug-only autoload that drives the visualize runtime update loop during editor/game execution.
/// </summary>
internal partial class Autoload : Node
{
    /// <summary>
    /// Owns registration and cleanup of visualize services for the lifetime of this autoload.
    /// </summary>
    private VisualizeAutoload _visualizeAutoload = null!;

    public override void _EnterTree()
    {
        _visualizeAutoload = new VisualizeAutoload();
    }

    public override void _Process(double delta)
    {
        Visualize.Update();
    }

    public override void _ExitTree()
    {
        _visualizeAutoload.Dispose();
    }
}
#endif
