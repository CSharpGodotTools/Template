using Framework.Netcode;
using Godot;

namespace Template.Setup.Testing;

public struct MovementSnapshot
{
    public int Tick { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
}

public partial class CPacketStructTypes : ClientPacket
{
    public MovementSnapshot Spawn { get; set; }
    public MovementSnapshot Current { get; set; }
    public MovementSnapshot Target { get; set; }
}
