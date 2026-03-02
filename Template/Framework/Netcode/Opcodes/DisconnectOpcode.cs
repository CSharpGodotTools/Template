namespace Framework.Netcode;

/// <summary>
/// Disconnect reason codes sent through ENet disconnection events.
/// </summary>
public enum DisconnectOpcode
{
    Disconnected,
    Maintenance,
    Restarting,
    Stopping,
    Timeout,
    Kicked,
    Banned
}
