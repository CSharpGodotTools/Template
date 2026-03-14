namespace __TEMPLATE__.Netcode;

/// <summary>
/// Wraps a client packet together with the peer ID of the sender.
/// Passed to server packet handlers so they receive all context in a single parameter.
/// </summary>
public readonly struct PacketFromPeer<TPacket>
    where TPacket : ClientPacket
{
    /// <summary>The deserialized packet sent by the peer.</summary>
    public TPacket Packet { get; init; }

    /// <summary>ENet peer ID of the sender.</summary>
    public uint PeerId { get; init; }
}
