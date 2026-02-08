using Framework.Netcode.Examples.Topdown;
using Godot;
using System;

namespace Tests;

public partial class Testing : Node
{
    public override void _Ready()
    {
        CPacketPlayerInfo p1 = new()
        {
            Username = "Valky",
            Position = new Vector2(100, 100)
        };

        CPacketPlayerInfo p2 = new()
        {
            Username = "Valky",
            Position = new Vector2(100, 100)
        };

        GD.Print(p1.Equals(p2));
    }
}
