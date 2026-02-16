using System;

namespace Framework.Netcode;

/// <summary>
/// Packet sent from a client to a server.
/// </summary>
public abstract class ClientPacket : GamePacket
{
    private readonly Type _packetType;

    /// <summary>
    /// Creates a client packet and caches its runtime type for opcode lookup.
    /// </summary>
    public ClientPacket()
    {
        _packetType = GetType();
    }

    /// <summary>
    /// Sends this packet to the configured target peer.
    /// </summary>
    public void Send()
    {
        if (Peers == null || Peers.Length == 0)
        {
            throw new InvalidOperationException($"{GetType().Name} cannot send without a target peer.");
        }

        ENet.Packet enetPacket = CreateENetPacket();
        Peers[0].Send(ChannelId, ref enetPacket);
    }

    /// <summary>
    /// Returns the registry opcode for this client packet type.
    /// </summary>
    public override byte GetOpcode()
    {
        return PacketRegistry.ClientPacketInfo[_packetType].Opcode;
    }
}
