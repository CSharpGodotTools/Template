using Godot;

namespace __TEMPLATE__;

public partial class DragDropBase : Node
{
    public override void _Ready()
    {
        DragUtils.AddChildArea(GetParent());
    }
}
