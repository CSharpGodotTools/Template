using Godot;

namespace __TEMPLATE__.UI;

[GlobalClass]
public partial class Scenes : Node
{
    [Export] public PackedScene Game { get; private set; }
}
