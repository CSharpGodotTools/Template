namespace Framework.Netcode.Server;

/// <summary>
/// Commands consumed by the ENet server worker queue.
/// </summary>
public enum ENetServerOpcode
{
    Stop,
    Kick,
    KickAll
}
