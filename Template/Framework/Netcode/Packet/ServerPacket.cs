using ENet;
using System;
using Framework.Netcode.Client;

namespace Framework.Netcode;

/// <summary>
/// Packet sent from a server to one or more clients.
/// </summary>
public abstract class ServerPacket : GamePacket
{
    private SendType _sendType;
    private readonly Type _packetType;

    /// <summary>
    /// Creates a server packet and caches its runtime type for opcode lookup.
    /// </summary>
    public ServerPacket()
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

        Packet enetPacket = CreateENetPacket();
        Peers[0].Send(ChannelId, ref enetPacket);
    }

    /// <summary>
    /// Broadcasts this packet through the host, optionally excluding peers.
    /// </summary>
    public void Broadcast(Host host)
    {
        ArgumentNullException.ThrowIfNull(host);

        Packet enetPacket = CreateENetPacket();
        Peer[] peers = Peers ?? [];

        if (peers.Length == 0)
        {
            host.Broadcast(ChannelId, ref enetPacket);
        }
        else if (peers.Length == 1)
        {
            host.Broadcast(ChannelId, ref enetPacket, peers[0]);
        }
        else
        {
            host.Broadcast(ChannelId, ref enetPacket, peers);
        }
    }

    /// <summary>
    /// Sets the delivery mode used by the server worker when sending this packet.
    /// </summary>
    public void SetSendType(SendType sendType)
    {
        _sendType = sendType;
    }

    /// <summary>
    /// Gets the current delivery mode for this packet.
    /// </summary>
    public SendType GetSendType()
    {
        return _sendType;
    }

    /// <summary>
    /// Returns the registry opcode for this server packet type.
    /// </summary>
    public override byte GetOpcode()
    {
        return PacketRegistry.ServerPacketInfo[_packetType].Opcode;
    }
}

/// <summary>
/// Delivery mode selected when enqueuing server packets.
/// </summary>
public enum SendType
{
    Peer,
    Broadcast
}
