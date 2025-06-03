using Godot;

namespace __TEMPLATE__.DragManager;

[GlobalClass]
public partial class DroppableComponent : DragDropBase
{
    [Export] public bool Animate { get; set; }
}
