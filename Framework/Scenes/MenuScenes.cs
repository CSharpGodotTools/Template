using Godot;

namespace GodotUtils.UI;

[GlobalClass]
public partial class MenuScenes : Resource
{
    [Export] public PackedScene MainMenu { get; private set; }
    [Export] public PackedScene ModLoader { get; private set; }
    [Export] public PackedScene Options { get; private set; }
    [Export] public PackedScene Credits { get; private set; }
}
