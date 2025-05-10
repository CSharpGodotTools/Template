using Godot;

namespace Template;

public partial class DragDropBase : Node
{
    public override void _Ready()
    {
        DragUtils.AddChildArea(GetParent());
    }
}
