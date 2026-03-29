using ENet;
using System;

namespace __TEMPLATE__.Netcode.Server;

internal sealed class ServerOutgoingProcessor
{
    private const byte DefaultChannelId = 0;

    private readonly ServerQueueManager _queues;
    private readonly ServerPeerStore _peers;
    private readonly Func<Host> _hostProvider;
    private readonly Func<ushort> _nextStreamId;
    private readonly Func<ushort> _maxFragmentsPerPacket;
    private readonly Func<byte[], Packet> _packetFactory;
    private readonly Action<Exception> _onSendError;

    public ServerOutgoingProcessor(
        ServerQueueManager queues,
        ServerPeerStore peers,
        Func<Host> hostProvider,
        Func<ushort> nextStreamId,
        Func<ushort> maxFragmentsPerPacket,
        Func<byte[], Packet> packetFactory,
        Action<Exception> onSendError)
    {
        _queues = queues;
        _peers = peers;
        _hostProvider = hostProvider;
        _nextStreamId = nextStreamId;
        _maxFragmentsPerPacket = maxFragmentsPerPacket;
        _packetFactory = packetFactory;
        _onSendError = onSendError;
    }

    public void Process()
    {
        while (_queues.TryDequeueOutgoing(out OutgoingMessage? message))
        {
            if (message == null)
                continue;

            try
            {
                if (message.IsBroadcast)
                    SendBroadcast(message);
                else
                    SendUnicast(message);
            }
            catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
            {
                _onSendError(exception);
            }
        }
    }

    private void SendUnicast(OutgoingMessage message)
    {
        if (!_peers.TryGetPeer(message.TargetPeerId, out Peer peer))
            return;

        if (message.Data.Length <= GamePacket.MaxSize)
        {
            Packet packet = _packetFactory(message.Data);
            peer.Send(DefaultChannelId, ref packet);
            return;
        }

        foreach (byte[] fragment in PacketFragmenter.Fragment(message.Data, _nextStreamId(), _maxFragmentsPerPacket()))
        {
            Packet packet = _packetFactory(fragment);
            peer.Send(DefaultChannelId, ref packet);
        }
    }

    private void SendBroadcast(OutgoingMessage message)
    {
        Host host = _hostProvider();

        if (message.Data.Length <= GamePacket.MaxSize)
        {
            Packet packet = _packetFactory(message.Data);
            BroadcastPacket(host, message, ref packet);
            return;
        }

        foreach (byte[] fragment in PacketFragmenter.Fragment(message.Data, _nextStreamId(), _maxFragmentsPerPacket()))
        {
            Packet packet = _packetFactory(fragment);
            BroadcastPacket(host, message, ref packet);
        }
    }

    private void BroadcastPacket(Host host, OutgoingMessage message, ref Packet packet)
    {
        if (message.HasExclusion && _peers.TryGetPeer(message.ExcludePeerId, out Peer excludedPeer))
            host.Broadcast(DefaultChannelId, ref packet, excludedPeer);
        else
            host.Broadcast(DefaultChannelId, ref packet);
    }
}
