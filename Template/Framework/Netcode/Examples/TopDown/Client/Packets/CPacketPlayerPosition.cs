using Godot;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

public partial class CPacketPlayerPosition : ClientPacket
{
    public Vector2 Position { get; set; }
}
