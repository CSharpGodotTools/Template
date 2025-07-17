using Godot;
using System.Collections.Generic;
using __TEMPLATE__.Netcode;
using __TEMPLATE__.Netcode.Client;

namespace __TEMPLATE__.TopDown2D;

public class SPacketPlayerPositions : ServerPacket
{
    [NetSend(1)]
    public Dictionary<uint, Vector2> Positions { get; set; }

    public override void Handle(ENetClient client)
    {
        Level level = Services.Get<Level>();

        foreach (KeyValuePair <uint, Vector2> pair in Positions)
        {
            if (level.OtherPlayers.TryGetValue(pair.Key, out OtherPlayer otherPlayer))
            {
                otherPlayer.LastServerPosition = pair.Value;
            }
        }

        // Send a client position packet to the server immediately right after
        // a server positions packet is received

        // Player could be invalid here if they disconnected or died
        if (GodotObject.IsInstanceValid(level.Player))
        {
            level.Player.NetSendPosition();
        }
    }
}
