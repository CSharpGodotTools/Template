namespace Template.Setup.Testing;

public sealed class PacketWaitDiagnostics
{
    public bool Received { get; set; }
    public bool SawOutgoingEnqueue { get; set; }
    public bool SawOutgoingDrain { get; set; }
    public int LastOutgoingCount { get; set; }
    public int LastGodotPacketCount { get; set; }
    public int LastCommandCount { get; set; }
    public bool ClientRunning { get; set; }
    public bool ClientConnected { get; set; }
    public bool ServerRunning { get; set; }
}
