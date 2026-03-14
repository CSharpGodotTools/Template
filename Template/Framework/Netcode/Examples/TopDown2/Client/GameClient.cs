using __TEMPLATE__.Netcode.Client;
using Godot;

namespace __TEMPLATE__.Netcode.Examples.TopDown2;

public class GameClient : GodotClient
{
    public GameClient()
    {
        // no packet handlers yet
    }

    protected override void OnConnected()
    {
        Send(new CPacketPlayerPosition(new Vector2(100, 100)));
    }
}
