using __TEMPLATE__.Netcode.Client;
using Godot;

namespace __TEMPLATE__.Netcode.Examples.TopDown2;

/// <summary>
/// Minimal sample client that sends an initial position packet after connecting.
/// </summary>
public class GameClient : GodotClient
{
    /// <summary>
    /// Creates the sample client instance.
    /// </summary>
    public GameClient()
    {
        // no packet handlers yet
    }

    /// <summary>
    /// Sends an initial test position to the server after connect.
    /// </summary>
    protected override void OnConnected()
    {
        Send(new CPacketPlayerPosition(new Vector2(100, 100)));
    }
}
