using Godot;

namespace __TEMPLATE__.Ui;

[GlobalClass]
/// <summary>
/// Resource container that maps main menu destinations to PackedScene assets.
/// </summary>
public partial class MenuScenes : Resource
{
    [Export] public PackedScene MainMenu { get; private set; } = null!;
    [Export] public PackedScene ModLoader { get; private set; } = null!;
    [Export] public PackedScene Options { get; private set; } = null!;
    [Export] public PackedScene Credits { get; private set; } = null!;
}
