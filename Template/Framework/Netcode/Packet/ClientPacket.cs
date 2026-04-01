using System;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Packet sent from a client to a server.
/// </summary>
public abstract class ClientPacket : GamePacket
{
    private readonly Type _packetType;

    /// <summary>
    /// Caches the concrete runtime type so opcode lookup does not allocate repeatedly.
    /// </summary>
    protected ClientPacket()
    {
        _packetType = GetType();
    }

    /// <summary>
    /// Returns the registry opcode for this client packet type.
    /// </summary>
    /// <returns>Opcode value registered for this client packet type.</returns>
    public override ushort GetOpcode()
    {
        return (ushort)PacketRegistry.ClientPacketInfo[_packetType].Opcode;
    }

    /// <summary>
    /// Writes the opcode with the exact wire width configured in <see cref="PacketRegistryAttribute"/>.
    /// </summary>
    /// <param name="writer">Packet writer receiving opcode bytes.</param>
    protected override void WriteOpcode(PacketWriter writer)
    {
        writer.Write(PacketRegistry.ClientPacketInfo[_packetType].Opcode);
    }
}
