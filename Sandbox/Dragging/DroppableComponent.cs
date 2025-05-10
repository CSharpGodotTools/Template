using Godot;
using System;

namespace Template;

[GlobalClass]
public partial class DroppableComponent : DragDropBase
{
    [Export] public bool Animate { get; set; }
}
