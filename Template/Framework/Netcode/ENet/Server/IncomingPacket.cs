using ENet;

namespace Framework.Netcode.Server;

public abstract partial class ENetServer
{
    private readonly struct IncomingPacket
    {
        public Packet Packet { get; init; }
        public Peer Peer { get; init; }
    }
}
