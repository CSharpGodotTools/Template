using Godot;

namespace __TEMPLATE__.Netcode.Examples.TopDown2;

/// <summary>
/// Client packet carrying player position updates for the TopDown2 sample.
/// </summary>
public partial class CPacketPlayerPosition : ClientPacket
{
    /// <summary>
    /// Gets or sets world position to publish to the server.
    /// </summary>
    public Vector2 Position { get; set; }
}
