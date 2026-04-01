using System;

namespace __TEMPLATE__.Netcode.Client;

/// <summary>
/// Envelope used by worker threads to hand decoded packet context to the Godot thread.
/// </summary>
public class PacketData
{
    /// <summary>
    /// Reusable packet instance used to deserialize payload data on the Godot thread.
    /// </summary>
    public required ServerPacket HandlePacket { get; set; }

    /// <summary>
    /// Reader instance owning the payload data for this packet.
    /// </summary>
    public required PacketReader PacketReader { get; set; }

    /// <summary>
    /// Packet runtime type used for handler dispatch.
    /// </summary>
    public required Type Type { get; set; }
}
