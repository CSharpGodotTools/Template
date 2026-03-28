namespace __TEMPLATE__.Netcode;

/// <summary>
/// Runtime logging and diagnostics options for ENet client/server wrappers.
/// </summary>
public class ENetOptions
{
    public bool PrintPacketData { get; set; } = false;
    public bool PrintPacketByteSize { get; set; } = false;
    public bool PrintPacketReceived { get; set; } = true;
    public bool PrintPacketSent { get; set; } = true;
    public bool ShowLogTimestamps { get; set; } = true;

    // Queue/backpressure limits.
    public int MaxCommandQueueDepth { get; set; } = 1024;
    public int MaxIncomingQueueDepth { get; set; } = 4096;
    public int MaxOutgoingQueueDepth { get; set; } = 4096;

    public QueueOverflowPolicy CommandQueueOverflowPolicy { get; set; } = QueueOverflowPolicy.DropNewest;
    public QueueOverflowPolicy IncomingQueueOverflowPolicy { get; set; } = QueueOverflowPolicy.DropOldest;
    public QueueOverflowPolicy OutgoingQueueOverflowPolicy { get; set; } = QueueOverflowPolicy.DropOldest;

    // Fragment validation/diagnostics.
    public ushort MaxFragmentsPerPacket { get; set; } = 1024;
    public int MalformedFragmentLogIntervalMs { get; set; } = 2000;
    public int QueueOverflowLogIntervalMs { get; set; } = 2000;
}
