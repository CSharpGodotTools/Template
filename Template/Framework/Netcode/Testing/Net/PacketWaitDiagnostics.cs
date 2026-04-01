namespace Template.Setup.Testing;

/// <summary>
/// Captures endpoint state sampled while waiting for packet arrival.
/// </summary>
public sealed class PacketWaitDiagnostics
{
    /// <summary>
    /// Gets or sets whether the expected packet was captured.
    /// </summary>
    public bool Received { get; set; }

    /// <summary>
    /// Gets or sets whether the client endpoint was still running.
    /// </summary>
    public bool ClientRunning { get; set; }

    /// <summary>
    /// Gets or sets whether the client endpoint was connected.
    /// </summary>
    public bool ClientConnected { get; set; }

    /// <summary>
    /// Gets or sets whether the server endpoint was still running.
    /// </summary>
    public bool ServerRunning { get; set; }
}
