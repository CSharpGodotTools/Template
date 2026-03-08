using Godot;

namespace Framework.Ui;

[GlobalClass]
public partial class Scenes : Node
{
    [Export] public PackedScene Game { get; private set; } = null!;
}
