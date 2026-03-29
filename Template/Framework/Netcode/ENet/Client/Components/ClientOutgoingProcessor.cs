using ENet;
using System;

namespace __TEMPLATE__.Netcode.Client;

internal sealed class ClientOutgoingProcessor
{
    private const byte DefaultChannelId = 0;

    private readonly ClientQueueManager _queues;
    private readonly Func<Peer> _peerProvider;
    private readonly Func<ushort> _nextStreamId;
    private readonly Func<ushort> _maxFragmentsPerPacket;
    private readonly Func<byte[], Packet> _packetFactory;
    private readonly Action<Exception> _onSendError;

    public ClientOutgoingProcessor(
        ClientQueueManager queues,
        Func<Peer> peerProvider,
        Func<ushort> nextStreamId,
        Func<ushort> maxFragmentsPerPacket,
        Func<byte[], Packet> packetFactory,
        Action<Exception> onSendError)
    {
        _queues = queues;
        _peerProvider = peerProvider;
        _nextStreamId = nextStreamId;
        _maxFragmentsPerPacket = maxFragmentsPerPacket;
        _packetFactory = packetFactory;
        _onSendError = onSendError;
    }

    public void Process()
    {
        while (_queues.TryDequeueOutgoing(out byte[]? data))
        {
            try
            {
                Send(data!);
            }
            catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
            {
                _onSendError(exception);
            }
        }
    }

    private void Send(byte[] data)
    {
        Peer peer = _peerProvider();

        if (data.Length <= GamePacket.MaxSize)
        {
            Packet packet = _packetFactory(data);
            peer.Send(DefaultChannelId, ref packet);
            return;
        }

        foreach (byte[] fragment in PacketFragmenter.Fragment(data, _nextStreamId(), _maxFragmentsPerPacket()))
        {
            Packet packet = _packetFactory(fragment);
            peer.Send(DefaultChannelId, ref packet);
        }
    }
}
