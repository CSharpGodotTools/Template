using ENet;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Couples a received ENet packet with its source peer.
/// </summary>
internal sealed class IncomingPacket
{

    /// <summary>
    /// Creates an incoming packet envelope.
    /// </summary>
    /// <param name="packet">Received ENet packet.</param>
    /// <param name="peer">Source peer that sent the packet.</param>
    public IncomingPacket(Packet packet, Peer peer)
    {
        Packet = packet;
        Peer = peer;
    }

    /// <summary>
    /// Gets the received ENet packet.
    /// </summary>
    public Packet Packet { get; }

    /// <summary>
    /// Gets the source peer.
    /// </summary>
    public Peer Peer { get; }
}
