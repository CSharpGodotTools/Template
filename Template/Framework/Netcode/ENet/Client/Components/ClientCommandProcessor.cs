using System;

namespace __TEMPLATE__.Netcode.Client;

internal sealed class ClientCommandProcessor
{
    private readonly ClientQueueManager _queues;
    private readonly Func<bool> _isStopping;
    private readonly Action<string> _log;
    private readonly Action<DisconnectOpcode> _disconnect;

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

    public void Process()
    {
        while (_queues.TryDequeueCommand(out Cmd<ENetClientOpcode>? command))
        {
            if (command == null)
                continue;

            if (command.Opcode == ENetClientOpcode.Disconnect)
                HandleDisconnectCommand();
        }
    }

    private void HandleDisconnectCommand()
    {
        if (_isStopping())
        {
            _log("Client is in the middle of stopping");
            return;
        }

        _disconnect(DisconnectOpcode.Disconnected);
    }
}
