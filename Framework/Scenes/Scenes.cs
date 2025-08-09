using Godot;
using System;

namespace __TEMPLATE__.UI;

[GlobalClass]
public partial class Scenes : Resource
{
    [Export] public PackedScene MainMenu { get; set; }
    [Export] public PackedScene ModLoader { get; set; }
    [Export] public PackedScene Options { get; set; }
    [Export] public PackedScene Credits { get; set; }
    [Export] public PackedScene Game { get; set; }
}
