using ENet;
using System.Collections.Generic;
using System;
using Template.Netcode.Client;

namespace Template.Netcode;

/// <summary>
/// A packet sent from the server to other client(s)
/// </summary>
public abstract class ServerPacket : GamePacket
{
    public static Dictionary<Type, PacketInfo<ServerPacket>> PacketMap { get; } = NetcodeUtils.MapPackets<ServerPacket>();
    public static Dictionary<byte, Type> PacketMapBytes { get; set; } = [];

    private SendType _sendType;

    public static void MapOpcodes()
    {
        foreach (KeyValuePair<Type, PacketInfo<ServerPacket>> packet in PacketMap)
        {
            PacketMapBytes.Add(packet.Value.Opcode, packet.Key);
        }
    }

    public void Send()
    {
        Packet enetPacket = CreateENetPacket();
        Peers[0].Send(ChannelId, ref enetPacket);
    }

    public void Broadcast(Host host)
    {
        Packet enetPacket = CreateENetPacket();

        if (Peers.Length == 0)
        {
            host.Broadcast(ChannelId, ref enetPacket);
        }
        else if (Peers.Length == 1)
        {
            host.Broadcast(ChannelId, ref enetPacket, Peers[0]);
        }
        else
        {
            host.Broadcast(ChannelId, ref enetPacket, Peers);
        }
    }

    public void SetSendType(SendType sendType)
    {
        _sendType = sendType;
    }

    public SendType GetSendType()
    {
        return _sendType;
    }

    public override byte GetOpcode()
    {
        return PacketMap[GetType()].Opcode;
    }

    /// <summary>
    /// The packet handled client-side (Godot thread)
    /// </summary>
    public abstract void Handle(ENetClient client);
}

public enum SendType
{
    Peer,
    Broadcast
}

