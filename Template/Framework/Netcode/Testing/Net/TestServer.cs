using __TEMPLATE__.Netcode;
using __TEMPLATE__.Netcode.Server;
using System;

namespace Template.Setup.Testing;

/// <summary>
/// Test server that forwards received packets to a supplied callback.
/// </summary>
/// <typeparam name="TPacket">Packet type handled by this server.</typeparam>
public sealed class TestServer<TPacket> : GodotServer
    where TPacket : ClientPacket
{
    private readonly Action<TPacket, uint> _onPacket = null!;

    /// <summary>
    /// Initializes a server that forwards packet/publisher data to a callback.
    /// </summary>
    /// <param name="onPacket">Callback invoked for each received packet.</param>
    public TestServer(Action<TPacket, uint> onPacket)
    {
        _onPacket = onPacket;
        // Register handler only when packet callback is provided.
        if (_onPacket != null)
        {

            // Register a single typed handler for this test packet type.
            OnPacket<TPacket>(HandlePacket);
        }
    }

    /// <summary>
    /// Handles incoming typed packets and forwards packet plus peer id.
    /// </summary>
    /// <param name="peer">Packet and sender metadata from the server transport.</param>
    private void HandlePacket(PacketFromPeer<TPacket> peer)
    {
        _onPacket(peer.Packet, peer.PeerId);
    }
}
