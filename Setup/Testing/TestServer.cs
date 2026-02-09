using ENet;
using Framework.Netcode.Server;
using System;

namespace Template.Setup.Testing;

public sealed class TestServer : GodotServer
{
    private readonly Action<CPacketNestedCollections, Peer> _onPacket;

    public TestServer(Action<CPacketNestedCollections, Peer> onPacket)
    {
        _onPacket = onPacket;
        if (_onPacket != null)
        {
            RegisterPacketHandler<CPacketNestedCollections>(HandlePacket);
        }
    }

    private void HandlePacket(CPacketNestedCollections packet, Peer peer)
    {
        _onPacket(packet, peer);
    }
}
