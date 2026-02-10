using ENet;
using Framework.Netcode;
using Framework.Netcode.Server;
using System;

namespace Template.Setup.Testing;

public sealed class TestServer<TPacket> : GodotServer
    where TPacket : ClientPacket
{
    private readonly Action<TPacket, Peer> _onPacket;

    public TestServer(Action<TPacket, Peer> onPacket)
    {
        _onPacket = onPacket;
        if (_onPacket != null)
        {
            RegisterPacketHandler<TPacket>(HandlePacket);
        }
    }

    private void HandlePacket(TPacket packet, Peer peer)
    {
        _onPacket(packet, peer);
    }
}
