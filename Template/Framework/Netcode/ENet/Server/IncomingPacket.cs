using ENet;

namespace __TEMPLATE__.Netcode.Server;

internal sealed class IncomingPacket
{
    private readonly Packet _packet;
    private readonly Peer _peer;

    public IncomingPacket(Packet packet, Peer peer)
    {
        _packet = packet;
        _peer = peer;
    }

    public Packet Packet => _packet;
    public Peer Peer => _peer;
}
