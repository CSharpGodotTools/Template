using ENet;
using System;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Drains server control commands and executes stop/kick operations against connected peers.
/// </summary>
internal sealed class ServerCommandProcessor
{
    private readonly ServerQueueManager _queues;
    private readonly ServerPeerStore _peers;
    private readonly Func<bool> _isStopping;
    private readonly Action _requestStop;
    private readonly Action<string> _log;
    private readonly Action<uint, Peer, DisconnectOpcode> _disconnectPeer;
    private readonly Action<DisconnectOpcode> _disconnectAll;

    /// <summary>
    /// Creates a processor for server control commands.
    /// </summary>
    /// <param name="queues">Queue manager that holds pending server commands.</param>
    /// <param name="peers">Peer store used for peer lookup during kick operations.</param>
    /// <param name="isStopping">Callback indicating whether shutdown is already in progress.</param>
    /// <param name="requestStop">Callback that requests server shutdown.</param>
    /// <param name="log">Logger callback for operator diagnostics.</param>
    /// <param name="disconnectPeer">Callback that disconnects a specific peer.</param>
    /// <param name="disconnectAll">Callback that disconnects all peers.</param>
    public ServerCommandProcessor(
        ServerQueueManager queues,
        ServerPeerStore peers,
        Func<bool> isStopping,
        Action requestStop,
        Action<string> log,
        Action<uint, Peer, DisconnectOpcode> disconnectPeer,
        Action<DisconnectOpcode> disconnectAll)
    {
        _queues = queues;
        _peers = peers;
        _isStopping = isStopping;
        _requestStop = requestStop;
        _log = log;
        _disconnectPeer = disconnectPeer;
        _disconnectAll = disconnectAll;
    }

    /// <summary>
    /// Processes all queued server control commands.
    /// </summary>
    public void Process()
    {
        while (_queues.TryDequeueCommand(out Cmd<ENetServerOpcode>? command))
        {
            // Skip null command entries that can appear during queue drain races.
            if (command == null)
                continue;

            switch (command.Opcode)
            {
                case ENetServerOpcode.Stop:
                    HandleStop();
                    break;

                case ENetServerOpcode.Kick:
                    HandleKick(command);
                    break;

                case ENetServerOpcode.KickAll:
                    HandleKickAll(command);
                    break;
            }
        }
    }

    /// <summary>
    /// Handles stop commands while preventing duplicate shutdown work.
    /// </summary>
    private void HandleStop()
    {
        // Ignore duplicate stop requests once shutdown is already underway.
        if (_isStopping())
        {
            _log("Server is in the middle of stopping");
            return;
        }

        _disconnectAll(DisconnectOpcode.Stopping);
        _requestStop();
    }

    /// <summary>
    /// Handles kick commands by resolving the target peer and issuing disconnect.
    /// </summary>
    /// <param name="command">Kick command payload.</param>
    private void HandleKick(Cmd<ENetServerOpcode> command)
    {
        uint peerId = (uint)command.Data[0];
        DisconnectOpcode opcode = (DisconnectOpcode)command.Data[1];

        // Reject kick requests for peers that are no longer tracked.
        if (!_peers.TryGetPeer(peerId, out Peer peer))
        {
            _log($"Tried to kick peer with id '{peerId}' but this peer does not exist");
            return;
        }

        _disconnectPeer(peerId, peer, opcode);
    }

    /// <summary>
    /// Handles kick-all commands.
    /// </summary>
    /// <param name="command">Kick-all command payload.</param>
    private void HandleKickAll(Cmd<ENetServerOpcode> command)
    {
        DisconnectOpcode opcode = (DisconnectOpcode)command.Data[0];
        _disconnectAll(opcode);
    }
}
