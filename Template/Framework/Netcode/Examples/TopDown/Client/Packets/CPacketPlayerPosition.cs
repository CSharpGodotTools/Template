using Godot;

namespace Framework.Netcode.Examples.Topdown;

public partial class CPacketPlayerPosition : ClientPacket
{
    public Vector2 Position { get; set; }
}
