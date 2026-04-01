using System;

namespace __TEMPLATE__.Netcode.Client;

/// <summary>
/// Consumes queued client control commands and translates them into connection actions.
/// </summary>
internal sealed class ClientCommandProcessor
{
    private readonly ClientQueueManager _queues;
    private readonly Func<bool> _isStopping;
    private readonly Action<string> _log;
    private readonly Action<DisconnectOpcode> _disconnect;

    /// <summary>
    /// Creates a command processor for client control opcodes.
    /// </summary>
    /// <param name="queues">Shared queue manager that stores pending commands.</param>
    /// <param name="isStopping">Callback that reports whether shutdown is already in progress.</param>
    /// <param name="log">Logger callback for operator-facing messages.</param>
    /// <param name="disconnect">Callback that executes disconnect logic.</param>
    public ClientCommandProcessor(
        ClientQueueManager queues,
        Func<bool> isStopping,
        Action<string> log,
        Action<DisconnectOpcode> disconnect)
    {
        _queues = queues;
        _isStopping = isStopping;
        _log = log;
        _disconnect = disconnect;
    }

    /// <summary>
    /// Drains queued commands and executes recognized client control operations.
    /// </summary>
    public void Process()
    {
        // Command processing is intentionally drain-until-empty to keep control latency low.
        while (_queues.TryDequeueCommand(out Cmd<ENetClientOpcode>? command))
        {
            // Skip null command entries that can occur during queue races.
            if (command == null)
                continue;

            // Handle only disconnect opcodes from the client command queue.
            if (command.Opcode == ENetClientOpcode.Disconnect)
                HandleDisconnectCommand();
        }
    }

    /// <summary>
    /// Handles a disconnect command while guarding against duplicate stop flows.
    /// </summary>
    private void HandleDisconnectCommand()
    {
        // Ignore duplicate disconnect commands once shutdown has started.
        if (_isStopping())
        {
            _log("Client is in the middle of stopping");
            return;
        }

        _disconnect(DisconnectOpcode.Disconnected);
    }
}
