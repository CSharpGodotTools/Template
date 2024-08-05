﻿namespace Template;

using ENet;
using GodotUtils.Netcode;
using GodotUtils.Netcode.Server;

public class CPacketJoin : ClientPacket
{
    public string Username { get; set; }

    public override void Write(PacketWriter writer)
    {
        writer.Write((string)Username);
    }

    public override void Read(PacketReader reader)
    {
        Username = reader.ReadString();
    }

    public override void Handle(ENetServer s, Peer client)
    {
        GameServer server = (GameServer)s;

        // Keep track of this new player server-side
        server.Players.Add(client.ID, new()
        {
            Username = Username
        });

        // Acknowledge connection and tell player about the other players
        server.Send(new SPacketPlayerConnectionAcknowledged
        {
            // Other players means all players except the player that just joined
            OtherPlayers = server.GetOtherPlayers(client.ID)
        }, client);

        // Tell everyone else about this new player
        foreach (KeyValuePair<uint, PlayerData> pair in server.GetOtherPlayers(client.ID))
        {
            server.Send(new SPacketPlayerJoinLeave
            {
                Id = client.ID,
                Username = server.Players[client.ID].Username,
                Position = server.Players[client.ID].Position,
                Joined = true
            }, server.Peers[pair.Key]);
        }
    }
}
