using ENet;
using System;

namespace Framework.Netcode;

/// <summary>
/// Shared packet functionality for client-to-server and server-to-client packets.
/// </summary>
public abstract class GamePacket
{
    public static int MaxSize => 8192;

    protected Peer[] Peers { get; private set; } = [];
    protected byte ChannelId { get; }

    private readonly PacketFlags _packetFlags = PacketFlags.Reliable;
    private long _size;
    private byte[] _data;

    /// <summary>
    /// Serializes opcode and payload into an ENet-compatible byte buffer.
    /// </summary>
    public void Write()
    {
        using PacketWriter writer = new();
        writer.Write(GetOpcode());
        Write(writer);

        _data = writer.Stream.ToArray();
        _size = writer.Stream.Length;
    }

    /// <summary>
    /// Sets a single target peer for this packet.
    /// </summary>
    public void SetPeer(Peer peer)
    {
        Peers = [peer];
    }

    /// <summary>
    /// Sets one or more target peers for this packet.
    /// </summary>
    public void SetPeers(Peer[] peers)
    {
        ArgumentNullException.ThrowIfNull(peers);

        Peers = [.. peers];
    }

    /// <summary>
    /// Gets the size of the most recently serialized packet payload.
    /// </summary>
    public long GetSize()
    {
        return _size;
    }

    /// <summary>
    /// Returns the opcode associated with this packet type.
    /// </summary>
    public abstract byte GetOpcode();

    /// <summary>
    /// Writes packet payload data after opcode serialization.
    /// PacketGen generates this path in packet partial classes, and reflection fallback should be avoided for new packets.
    /// </summary>
    public virtual void Write(PacketWriter writer)
    {
        // Implemented in generated packet partials.
    }

    /// <summary>
    /// Reads packet payload data after opcode deserialization.
    /// PacketGen generates this path in packet partial classes, and reflection fallback should be avoided for new packets.
    /// </summary>
    public virtual void Read(PacketReader reader)
    {
        // Implemented in generated packet partials.
    }

    /// <summary>
    /// Creates an ENet packet from the serialized payload.
    /// </summary>
    protected Packet CreateENetPacket()
    {
        if (_data == null)
        {
            throw new InvalidOperationException($"{GetType().Name} cannot create an ENet packet before Write() is called.");
        }

        Packet enetPacket = default;
        enetPacket.Create(_data, _packetFlags);
        return enetPacket;
    }
}
