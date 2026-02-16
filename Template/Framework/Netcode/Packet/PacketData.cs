using System;

namespace Framework.Netcode.Client;

public class PacketData
{
    /// <summary>
    /// Reusable packet instance used to deserialize payload data on the Godot thread.
    /// </summary>
    public ServerPacket HandlePacket { get; set; }

    /// <summary>
    /// Reader instance owning the payload data for this packet.
    /// </summary>
    public PacketReader PacketReader { get; set; }

    /// <summary>
    /// Packet runtime type used for handler dispatch.
    /// </summary>
    public Type Type { get; set; }
}
