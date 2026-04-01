using ENet;
using System;

namespace __TEMPLATE__.Netcode.Client;

/// <summary>
/// Drains outgoing payloads, fragments oversized packets, and sends them through the active ENet peer.
/// </summary>
internal sealed class ClientOutgoingProcessor
{
    private const byte DefaultChannelId = 0;

    private readonly ClientQueueManager _queues;
    private readonly Func<Peer> _peerProvider;
    private readonly Func<ushort> _nextStreamId;
    private readonly Func<ushort> _maxFragmentsPerPacket;
    private readonly Func<byte[], Packet> _packetFactory;
    private readonly Action<Exception> _onSendError;

    /// <summary>
    /// Creates an outgoing packet processor for a client worker.
    /// </summary>
    /// <param name="queues">Queue manager containing pending outgoing payloads.</param>
    /// <param name="peerProvider">Callback resolving the current connected peer.</param>
    /// <param name="nextStreamId">Stream id generator for fragmented transmissions.</param>
    /// <param name="maxFragmentsPerPacket">Configured fragment cap provider.</param>
    /// <param name="packetFactory">Factory creating reliable ENet packets from payload bytes.</param>
    /// <param name="onSendError">Error callback for non-fatal send exceptions.</param>
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

    /// <summary>
    /// Sends all queued outgoing payloads for the current worker tick.
    /// </summary>
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

    /// <summary>
    /// Sends a payload directly or as a fragment sequence when payload size exceeds packet limits.
    /// </summary>
    /// <param name="data">Serialized outgoing packet payload.</param>
    private void Send(byte[] data)
    {
        Peer peer = _peerProvider();

        // Fast path for payloads already within protocol packet limits.
        if (data.Length <= GamePacket.MaxSize)
        {
            Packet packet = _packetFactory(data);
            peer.Send(DefaultChannelId, ref packet);
            return;
        }

        // Fragment oversized payloads into independently reliable ENet packets.
        foreach (byte[] fragment in PacketFragmenter.Fragment(data, _nextStreamId(), _maxFragmentsPerPacket()))
        {
            Packet packet = _packetFactory(fragment);
            peer.Send(DefaultChannelId, ref packet);
        }
    }
}
