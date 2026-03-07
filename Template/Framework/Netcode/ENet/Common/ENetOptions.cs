namespace Framework.Netcode;

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
}
