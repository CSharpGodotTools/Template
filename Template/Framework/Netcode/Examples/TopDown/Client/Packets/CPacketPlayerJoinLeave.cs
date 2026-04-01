namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// Client packet announcing player join/leave lifecycle in the TopDown sample.
/// </summary>
public partial class CPacketPlayerJoinLeave : ClientPacket
{
    /// <summary>
    /// Gets or sets whether the player is joining (<see langword="true"/>) or leaving (<see langword="false"/>).
    /// </summary>
    public bool Joined { get; set; }
}
