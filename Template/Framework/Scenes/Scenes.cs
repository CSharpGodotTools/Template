using Godot;

namespace Framework.UI;

[GlobalClass]
public partial class Scenes : Node
{
    [Export] public PackedScene Game { get; private set; }
}
