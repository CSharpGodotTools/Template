namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// Server packet announcing player join/leave lifecycle changes.
/// </summary>
public partial class SPacketPlayerJoinedLeaved : ServerPacket
{
    /// <summary>
    /// Gets or sets player id associated with this lifecycle event.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Gets or sets whether the player joined (<see langword="true"/>) or left (<see langword="false"/>).
    /// </summary>
    public bool Joined { get; set; }

    /// <summary>
    /// Gets or sets whether this event refers to the receiving client's own player.
    /// </summary>
    public bool IsLocal { get; set; }
}
