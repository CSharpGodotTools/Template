using Godot;
using System.Collections.Generic;

namespace Framework.Netcode.Examples.Topdown;

public partial class SPacketPlayerPositions : ServerPacket
{
    public Dictionary<uint, Vector2> Positions { get; set; }
}
