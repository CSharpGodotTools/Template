namespace Template.Setup.Testing;

public sealed class PacketWaitDiagnostics
{
    public bool Received { get; set; }
    public bool ClientRunning { get; set; }
    public bool ClientConnected { get; set; }
    public bool ServerRunning { get; set; }
}
