namespace __TEMPLATE__.Netcode;

/// <summary>
/// Lightweight command envelope passed across worker and main-thread queues.
/// </summary>
/// <typeparam name="TOpcode">Opcode enum/type identifying command meaning.</typeparam>
/// <param name="opcode">Opcode that identifies the queued command.</param>
/// <param name="data">Optional command payload arguments.</param>
public class Cmd<TOpcode>(TOpcode opcode, params object[] data)
{
    /// <summary>
    /// Gets or sets the opcode that identifies the queued command.
    /// </summary>
    public TOpcode Opcode { get; set; } = opcode;

    /// <summary>
    /// Gets or sets optional command payload arguments.
    /// </summary>
    public object[] Data { get; set; } = data;
}
