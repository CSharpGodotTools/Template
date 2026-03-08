using Godot;

namespace Framework.Netcode.Examples.TopDown2;

public partial class CPacketPlayerPosition : ClientPacket
{
    public Vector2 Position { get; set; }
}
