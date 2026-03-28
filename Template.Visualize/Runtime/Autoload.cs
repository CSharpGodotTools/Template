#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal partial class Autoload : Node
{
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
