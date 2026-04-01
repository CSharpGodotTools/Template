using ENet;
using System;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Drains outgoing server messages, fragments oversized payloads, and delivers unicast/broadcast packets.
/// </summary>
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

    /// <summary>
    /// Creates an outgoing processor for server transport messages.
    /// </summary>
    /// <param name="queues">Queue manager that stores pending outgoing messages.</param>
    /// <param name="peers">Peer store for unicast/broadcast exclusion lookup.</param>
    /// <param name="hostProvider">Callback returning current ENet host.</param>
    /// <param name="nextStreamId">Fragment stream id generator.</param>
    /// <param name="maxFragmentsPerPacket">Configured fragment cap provider.</param>
    /// <param name="packetFactory">Factory creating reliable ENet packets from payloads.</param>
    /// <param name="onSendError">Error callback for non-fatal send failures.</param>
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

    /// <summary>
    /// Processes all queued outgoing messages for this worker tick.
    /// </summary>
    public void Process()
    {
        while (_queues.TryDequeueOutgoing(out OutgoingMessage? message))
        {
            // Skip null queue entries that can appear during shutdown races.
            if (message == null)
                continue;

            try
            {
                // Route messages to broadcast or unicast delivery paths.
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

    /// <summary>
    /// Sends a unicast message to a target peer.
    /// </summary>
    /// <param name="message">Outgoing unicast envelope.</param>
    private void SendUnicast(OutgoingMessage message)
    {
        // Ignore unicast messages targeting peers that are no longer connected.
        if (!_peers.TryGetPeer(message.TargetPeerId, out Peer peer))
            return;

        // Fast path for payloads already within packet size limits.
        if (message.Data.Length <= GamePacket.MaxSize)
        {
            Packet packet = _packetFactory(message.Data);
            peer.Send(DefaultChannelId, ref packet);
            return;
        }

        // Fragment oversized payloads before sending to the peer.
        foreach (byte[] fragment in PacketFragmenter.Fragment(message.Data, _nextStreamId(), _maxFragmentsPerPacket()))
        {
            Packet packet = _packetFactory(fragment);
            peer.Send(DefaultChannelId, ref packet);
        }
    }

    /// <summary>
    /// Sends a broadcast message to all peers, optionally excluding one peer.
    /// </summary>
    /// <param name="message">Outgoing broadcast envelope.</param>
    private void SendBroadcast(OutgoingMessage message)
    {
        Host host = _hostProvider();

        // Fast path for payloads already within packet size limits.
        if (message.Data.Length <= GamePacket.MaxSize)
        {
            Packet packet = _packetFactory(message.Data);
            BroadcastPacket(host, message, ref packet);
            return;
        }

        // Fragment oversized payloads before host broadcast.
        foreach (byte[] fragment in PacketFragmenter.Fragment(message.Data, _nextStreamId(), _maxFragmentsPerPacket()))
        {
            Packet packet = _packetFactory(fragment);
            BroadcastPacket(host, message, ref packet);
        }
    }

    /// <summary>
    /// Broadcasts a packet through ENet host with optional exclusion.
    /// </summary>
    /// <param name="host">Active ENet host.</param>
    /// <param name="message">Outgoing broadcast envelope.</param>
    /// <param name="packet">Packet to broadcast.</param>
    private void BroadcastPacket(Host host, OutgoingMessage message, ref Packet packet)
    {
        // Use exclusion broadcast only when the excluded peer is still tracked.
        if (message.HasExclusion && _peers.TryGetPeer(message.ExcludePeerId, out Peer excludedPeer))
            host.Broadcast(DefaultChannelId, ref packet, excludedPeer);
        else
            host.Broadcast(DefaultChannelId, ref packet);
    }
}
