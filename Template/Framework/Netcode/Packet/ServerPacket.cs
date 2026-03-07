using System;

namespace Framework.Netcode;

/// <summary>
/// Packet sent from a server to one or more clients.
/// </summary>
public abstract class ServerPacket : GamePacket
{
    private readonly Type _packetType;

    protected ServerPacket()
    {
        _packetType = GetType();
    }

    /// <summary>
    /// Returns the registry opcode for this server packet type.
    /// </summary>
    public override ushort GetOpcode()
    {
        return (ushort)PacketRegistry.ServerPacketInfo[_packetType].Opcode;
    }

    /// <summary>
    /// Writes the opcode with the exact wire width configured in <see cref="PacketRegistryAttribute"/>.
    /// </summary>
    protected override void WriteOpcode(PacketWriter writer)
    {
        writer.Write(PacketRegistry.ServerPacketInfo[_packetType].Opcode);
    }
}
