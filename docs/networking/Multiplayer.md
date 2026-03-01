## Quick Start
Create the following packet scripts.

```cs
public partial class CPlayerJoined : ClientPacket
{
    public string Username { get; set; }
}
```

```cs
public partial class SPlayerPositions : ServerPacket
{
    public Dictionary<uint, Vector2> Positions { get; set; }
}
```

Create a script with the following that extends from `GodotClient`.

```cs
public partial class GameClient : GodotClient
{
    public GameClient()
    {
        RegisterPacketHandler<SPlayerPositions>(packet =>
        {
            Log($"Received {packet.Positions.Count} player positions");
        });
    }

    protected override void OnConnect(Event netEvent)
    {
        Send(new CPlayerJoined
        {
            Username = "Valk"
        });
    }
}
```

Create a script with the following that extends from `GodotServer`.

```cs
public partial class GameServer : GodotServer
{
    public Dictionary<uint, Vector2> Players { get; } = [];

    public GameServer()
    {
        RegisterPacketHandler<CPlayerJoined>(OnPlayerJoined);
    }

    private void OnPlayerJoined(CPlayerJoined packet, Peer peer)
    {
        Players[peer.ID] = Vector2.Zero;

        Send(new SPlayerPositions
        {
            Positions = Players
        }, peer);
    }

    protected override void OnPeerDisconnect(Event netEvent)
    {
        Players.Remove(netEvent.Peer.ID);
    }
}
```

To start and stop the client and server, create a UI script that extends `NetControlPanelLow<GameClient, GameServer>`.

Attach it to a UI node you design yourself and assign the exported fields (IP input, buttons, etc.)

```cs
public partial class NetControlPanel : NetControlPanelLow<GameClient, GameServer>
{
    protected override ENetOptions Options { get; set; } = new()
    {
        PrintPacketByteSize = true,
        PrintPacketData = true,
        PrintPacketReceived = true,
        PrintPacketSent = true
    };
}
```

## Minimal Packet Data Sent
Notice the `SPacketHello` packet has only 12 bytes. 10 for the characters in "What's up?", 1 to identify the opcode and 1 more because ENet has 1 byte of overhead. The data is not being serialized as a string, and the method name is not being added to the packet data either, only the raw data is being sent over in the most compact way.

<img width="462" height="192" alt="image" src="https://github.com/user-attachments/assets/8cf2e996-f065-4fe0-b3a9-0c7f79c699a8" />

> [!IMPORTANT]
> MacOS has not been tested and may require a [[Custom ENet Build|Custom-ENet-Builds]].

## Further Reading
- [Packets](Packets)
- [Client API](Client-API)
- [Server API](Server-API)
- [Common Mistakes](Netcode-Common-Mistakes)
- [ENet-CSharp Documentation](https://github.com/nxrighthere/ENet-CSharp?tab=readme-ov-file#api-reference)

[https://github.com/user-attachments/assets/964ced37-4a20-4de8-87ee-550fe5ecb561](https://github.com/user-attachments/assets/964ced37-4a20-4de8-87ee-550fe5ecb561)
