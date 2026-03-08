using Framework.Netcode.Client;
using Godot;

namespace Framework.Netcode.Examples.TopDown2;

public class GameClient : GodotClient
{
    protected override void RegisterPackets()
    {
        
    }

    protected override void OnConnected()
    {
        Send(new CPacketPlayerPosition(new Vector2(100, 100)));
    }
}
