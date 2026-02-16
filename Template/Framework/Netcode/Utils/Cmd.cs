namespace Framework.Netcode;

/// <summary>
/// Lightweight command envelope passed across worker and main-thread queues.
/// </summary>
public class Cmd<TOpcode>(TOpcode opcode, params object[] data)
{
    public TOpcode Opcode { get; set; } = opcode;
    public object[] Data { get; set; } = data;
}
