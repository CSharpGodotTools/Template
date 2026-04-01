using Godot;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// Client packet carrying local player position updates in the TopDown sample.
/// </summary>
public partial class CPacketPlayerPosition : ClientPacket
{
    /// <summary>
    /// Gets or sets current world position of the local player.
    /// </summary>
    public Vector2 Position { get; set; }
}
