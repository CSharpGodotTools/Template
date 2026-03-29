using ENet;
using System;

namespace __TEMPLATE__.Netcode.Server;

internal sealed class ServerCommandProcessor
{
    private readonly ServerQueueManager _queues;
    private readonly ServerPeerStore _peers;
    private readonly Func<bool> _isStopping;
    private readonly Action _requestStop;
    private readonly Action<string> _log;
    private readonly Action<uint, Peer, DisconnectOpcode> _disconnectPeer;
    private readonly Action<DisconnectOpcode> _disconnectAll;

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

    public void Process()
    {
        while (_queues.TryDequeueCommand(out Cmd<ENetServerOpcode>? command))
        {
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

    private void HandleStop()
    {
        if (_isStopping())
        {
            _log("Server is in the middle of stopping");
            return;
        }

        _disconnectAll(DisconnectOpcode.Stopping);
        _requestStop();
    }

    private void HandleKick(Cmd<ENetServerOpcode> command)
    {
        uint peerId = (uint)command.Data[0];
        DisconnectOpcode opcode = (DisconnectOpcode)command.Data[1];

        if (!_peers.TryGetPeer(peerId, out Peer peer))
        {
            _log($"Tried to kick peer with id '{peerId}' but this peer does not exist");
            return;
        }

        _disconnectPeer(peerId, peer, opcode);
    }

    private void HandleKickAll(Cmd<ENetServerOpcode> command)
    {
        DisconnectOpcode opcode = (DisconnectOpcode)command.Data[0];
        _disconnectAll(opcode);
    }
}
