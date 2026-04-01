namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// TopDown-specific network control panel with predefined debug-friendly ENet options.
/// </summary>
public partial class NetControlPanel : NetControlPanelLow<GameClient, GameServer>
{
    private const bool VerbosePacketLogs = false;
    private const int TopDownDefaultMaxClients = 500;

    /// <summary>
    /// Gets default max clients used when starting the sample server from UI.
    /// </summary>
    protected override int DefaultMaxClients { get; } = TopDownDefaultMaxClients;

    /// <summary>
    /// Gets ENet options used by this panel's start-server flow.
    /// </summary>
    protected override ENetOptions Options { get; set; } = new()
    {
        PrintPacketByteSize = VerbosePacketLogs,
        PrintPacketData = VerbosePacketLogs,
        PrintPacketReceived = VerbosePacketLogs,
        PrintPacketSent = VerbosePacketLogs
    };
}
