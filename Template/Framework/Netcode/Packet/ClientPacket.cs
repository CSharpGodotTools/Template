using System;

namespace Framework.Netcode;

/// <summary>
/// Packet sent from a client to a server.
/// </summary>
public abstract class ClientPacket : GamePacket
{
    private readonly Type _packetType;

    protected ClientPacket()
    {
        _packetType = GetType();
    }

    /// <summary>
    /// Returns the registry opcode for this client packet type.
    /// </summary>
    public override ushort GetOpcode()
    {
        return (ushort)PacketRegistry.ClientPacketInfo[_packetType].Opcode;
    }

    /// <summary>
    /// Writes the opcode with the exact wire width configured in <see cref="PacketRegistryAttribute"/>.
    /// </summary>
    protected override void WriteOpcode(PacketWriter writer)
    {
        writer.Write(PacketRegistry.ClientPacketInfo[_packetType].Opcode);
    }
}
