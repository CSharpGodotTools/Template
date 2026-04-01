namespace __TEMPLATE__.Netcode;

/// <summary>
/// Runtime logging and diagnostics options for ENet client/server wrappers.
/// </summary>
public class ENetOptions
{
    /// <summary>
    /// Gets or sets whether packet payload objects are logged using formatted output.
    /// </summary>
    public bool PrintPacketData { get; set; } = false;

    /// <summary>
    /// Gets or sets whether packet byte sizes are appended to packet send/receive logs.
    /// </summary>
    public bool PrintPacketByteSize { get; set; } = false;

    /// <summary>
    /// Gets or sets whether received packets are logged.
    /// </summary>
    public bool PrintPacketReceived { get; set; } = true;

    /// <summary>
    /// Gets or sets whether sent packets are logged.
    /// </summary>
    public bool PrintPacketSent { get; set; } = true;

    /// <summary>
    /// Gets or sets whether log messages include timestamp prefixes.
    /// </summary>
    public bool ShowLogTimestamps { get; set; } = true;

    // Queue/backpressure limits.
    /// <summary>
    /// Gets or sets max command queue depth before overflow policy applies.
    /// </summary>
    public int MaxCommandQueueDepth { get; set; } = 1024;

    /// <summary>
    /// Gets or sets max incoming queue depth before overflow policy applies.
    /// </summary>
    public int MaxIncomingQueueDepth { get; set; } = 4096;

    /// <summary>
    /// Gets or sets max outgoing queue depth before overflow policy applies.
    /// </summary>
    public int MaxOutgoingQueueDepth { get; set; } = 4096;

    /// <summary>
    /// Gets or sets overflow handling policy for command queue pressure.
    /// </summary>
    public QueueOverflowPolicy CommandQueueOverflowPolicy { get; set; } = QueueOverflowPolicy.DropNewest;

    /// <summary>
    /// Gets or sets overflow handling policy for incoming queue pressure.
    /// </summary>
    public QueueOverflowPolicy IncomingQueueOverflowPolicy { get; set; } = QueueOverflowPolicy.DropOldest;

    /// <summary>
    /// Gets or sets overflow handling policy for outgoing queue pressure.
    /// </summary>
    public QueueOverflowPolicy OutgoingQueueOverflowPolicy { get; set; } = QueueOverflowPolicy.DropOldest;

    // Fragment validation/diagnostics.
    /// <summary>
    /// Gets or sets maximum allowed fragments per logical packet.
    /// </summary>
    public ushort MaxFragmentsPerPacket { get; set; } = 1024;

    /// <summary>
    /// Gets or sets throttle interval for malformed fragment log messages.
    /// </summary>
    public int MalformedFragmentLogIntervalMs { get; set; } = 2000;

    /// <summary>
    /// Gets or sets throttle interval for queue overflow log messages.
    /// </summary>
    public int QueueOverflowLogIntervalMs { get; set; } = 2000;
}
