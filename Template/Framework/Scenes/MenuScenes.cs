using Godot;

namespace Framework.Ui;

[GlobalClass]
public partial class MenuScenes : Resource
{
    [Export] public PackedScene MainMenu  { get; private set; } = null!;
    [Export] public PackedScene ModLoader { get; private set; } = null!;
    [Export] public PackedScene Options   { get; private set; } = null!;
    [Export] public PackedScene Credits   { get; private set; } = null!;
}
