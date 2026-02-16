namespace Framework.Netcode.Client;

/// <summary>
/// Main-thread client lifecycle commands raised by the ENet client worker.
/// </summary>
public enum GodotOpcode
{
    Connected,
    Timeout,
    Disconnected
}
