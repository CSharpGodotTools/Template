using System;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Packet sent from a server to one or more clients.
/// </summary>
public abstract class ServerPacket : GamePacket
{
    private readonly Type _packetType;

    /// <summary>
    /// Initializes the packet and caches the runtime type used for registry lookups.
    /// </summary>
    protected ServerPacket()
    {
        _packetType = GetType();
    }

    /// <summary>
    /// Returns the registry opcode for this server packet type.
    /// </summary>
    /// <returns>Opcode value registered for this server packet type.</returns>
    public override ushort GetOpcode()
    {
        return (ushort)PacketRegistry.ServerPacketInfo[_packetType].Opcode;
    }

    /// <summary>
    /// Writes the opcode with the exact wire width configured in <see cref="PacketRegistryAttribute"/>.
    /// </summary>
    /// <param name="writer">Packet writer receiving opcode bytes.</param>
    protected override void WriteOpcode(PacketWriter writer)
    {
        writer.Write(PacketRegistry.ServerPacketInfo[_packetType].Opcode);
    }
}
