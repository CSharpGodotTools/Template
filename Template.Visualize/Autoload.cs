#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal partial class Autoload : Node
{
    private VisualizeAutoload _visualizeAutoload;

    public override void _EnterTree()
    {
        _visualizeAutoload = new VisualizeAutoload();
    }

    public override void _Process(double delta)
    {
        _visualizeAutoload.Update();
    }

    public override void _ExitTree()
    {
        _visualizeAutoload.Dispose();
    }
}
#endif
