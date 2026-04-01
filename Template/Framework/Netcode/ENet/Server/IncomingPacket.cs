using ENet;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Couples a received ENet packet with its source peer.
/// </summary>
internal sealed class IncomingPacket
{
    private readonly Packet _packet;
    private readonly Peer _peer;

    /// <summary>
    /// Creates an incoming packet envelope.
    /// </summary>
    /// <param name="packet">Received ENet packet.</param>
    /// <param name="peer">Source peer that sent the packet.</param>
    public IncomingPacket(Packet packet, Peer peer)
    {
        _packet = packet;
        _peer = peer;
    }

    /// <summary>
    /// Gets the received ENet packet.
    /// </summary>
    public Packet Packet => _packet;

    /// <summary>
    /// Gets the source peer.
    /// </summary>
    public Peer Peer => _peer;
}
