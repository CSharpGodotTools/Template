using Framework.Netcode;
using Framework.Netcode.Server;
using System;

namespace Template.Setup.Testing;

public sealed class TestServer<TPacket> : GodotServer
    where TPacket : ClientPacket
{
    private readonly Action<TPacket, uint> _onPacket = null!;

    public TestServer(Action<TPacket, uint> onPacket)
    {
        _onPacket = onPacket;
        if (_onPacket != null)
        {
            OnPacket<TPacket>(HandlePacket);
        }
    }


    private void HandlePacket(PacketFromPeer<TPacket> peer)
    {
        _onPacket(peer.Packet, peer.PeerId);
    }
}
