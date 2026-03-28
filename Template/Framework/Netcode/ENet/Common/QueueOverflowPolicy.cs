namespace __TEMPLATE__.Netcode;

/// <summary>
/// Overflow behavior used when ENet worker queues reach their configured capacity.
/// </summary>
public enum QueueOverflowPolicy
{
    DropOldest,
    DropNewest,
    DisconnectNoisyPeer
}
