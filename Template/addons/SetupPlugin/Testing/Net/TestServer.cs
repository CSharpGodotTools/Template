using Framework.Netcode;
using Framework.Netcode.Server;
using System;

namespace Template.Setup.Testing;

public sealed class TestServer<TPacket> : GodotServer
    where TPacket : ClientPacket
{
    private readonly Action<TPacket, uint> _onPacket;

    public TestServer(Action<TPacket, uint> onPacket)
    {
        _onPacket = onPacket;
        if (_onPacket != null)
        {
            OnPacket<TPacket>(HandlePacket);
        }
    }

    protected override void RegisterPackets()
    {
        
    }

    private void HandlePacket(TPacket packet, uint peerId)
    {
        _onPacket(packet, peerId);
    }
}
