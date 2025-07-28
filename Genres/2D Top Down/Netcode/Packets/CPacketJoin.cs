using Godot;
using System.Collections.Generic;
using System.Linq;
using ENet;
using __TEMPLATE__.Netcode;
using __TEMPLATE__.Netcode.Server;

namespace __TEMPLATE__.TopDown2D;

public class CPacketJoin : ClientPacket
{
    [NetSend(1)]
    public string Username { get; set; }

    [NetSend(2)]
    public Vector2 Position { get; set; }

    public override void Handle(ENetServer s, Peer client)
    {
        GameServer server = (GameServer)s;

        // Keep track of this new player server-side
        server.Players.Add(client.ID, new()
        {
            Username = Username,
            Position = Position
        });

        // Acknowledge connection and tell player about the other players
        server.Send(new SPacketPlayerConnectionAcknowledged
        {
            // Other players means all players except the player that just joined
            OtherPlayers = server.GetOtherPlayers(client.ID)
                .ToDictionary(x => x.Key, x => x.Value)
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

