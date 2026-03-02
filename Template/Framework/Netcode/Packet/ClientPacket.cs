using System;

namespace Framework.Netcode;

/// <summary>
/// Packet sent from a client to a server.
/// </summary>
public abstract class ClientPacket : GamePacket
{
    private readonly Type _packetType;

    public ClientPacket()
    {
        _packetType = GetType();
    }

    /// <summary>
    /// Returns the registry opcode for this client packet type.
    /// </summary>
    public override byte GetOpcode()
    {
        return PacketRegistry.ClientPacketInfo[_packetType].Opcode;
    }
}
