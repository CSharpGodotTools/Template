using System.Collections.Generic;
using System;
using Template.Netcode.Server;

namespace Template.Netcode;


public abstract class ClientPacket : GamePacket
{
    public static Dictionary<Type, PacketInfo<ClientPacket>> PacketMap { get; } = NetcodeUtils.MapPackets<ClientPacket>();
    public static Dictionary<byte, Type> PacketMapBytes { get; set; } = new();

    public static void MapOpcodes()
    {
        foreach (KeyValuePair<Type, PacketInfo<ClientPacket>> packet in PacketMap)
            PacketMapBytes.Add(packet.Value.Opcode, packet.Key);
    }

    public void Send()
    {
        ENet.Packet enetPacket = CreateENetPacket();
        Peers[0].Send(ChannelId, ref enetPacket);
    }

    public override byte GetOpcode() => PacketMap[GetType()].Opcode;

    /// <summary>
    /// The packet handled server-side
    /// </summary>
    public abstract void Handle(ENetServer server, ENet.Peer client);
}

