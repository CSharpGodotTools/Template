using System.Collections.Generic;
using __TEMPLATE__.Netcode;
using __TEMPLATE__.Netcode.Client;

namespace __TEMPLATE__.TopDown2D;

/// <summary>
/// The server has acknowledged this players connection. The server is informing
/// the client of this.
/// </summary>
public class SPacketPlayerConnectionAcknowledged : ServerPacket
{
    [NetSend(1)]
    public Dictionary<uint, PlayerData> OtherPlayers { get; set; }

    public override void Handle(ENetClient client)
    {
        client.Log("Client received server acknowledgement");

        Level level = Services.Get<Level>();

        level.AddLocalPlayer();

        foreach (KeyValuePair<uint, PlayerData> pair in OtherPlayers)
        {
            level.AddOtherPlayer(pair.Key, pair.Value);
        }
    }
}

