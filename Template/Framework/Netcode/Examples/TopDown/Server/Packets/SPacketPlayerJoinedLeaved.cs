namespace Framework.Netcode.Examples.Topdown;

public partial class SPacketPlayerJoinedLeaved : ServerPacket
{
    public uint Id { get; set; }
    public bool Joined { get; set; }
    public bool IsLocal { get; set; }
}
