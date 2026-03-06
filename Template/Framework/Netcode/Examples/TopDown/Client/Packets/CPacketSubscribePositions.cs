namespace Framework.Netcode.Examples.Topdown;

/// <summary>
/// Sent by a client to opt in to receiving periodic position broadcasts from the server.
/// Clients that omit this packet will not receive <see cref="SPacketPlayerPositions"/> updates,
/// which is the intended behaviour for bots that do not need remote position data.
/// </summary>
public partial class CPacketSubscribePositions : ClientPacket
{
}
