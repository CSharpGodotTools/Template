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
        return PacketRegistry.ServerPacketInfo[_packetType].Opcode;
    }
}
