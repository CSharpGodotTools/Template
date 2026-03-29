using ENet;
using System.Collections.Generic;
using System.Threading;

namespace __TEMPLATE__.Netcode.Server;

internal sealed class ServerPeerStore
{
    private readonly Dictionary<uint, Peer> _peers = [];
    private readonly Dictionary<uint, Dictionary<ushort, FragmentBuffer>> _reassemblyBuffers = [];
    private int _connectedPeerCount;

    public int ConnectedPeerCount => Interlocked.CompareExchange(ref _connectedPeerCount, 0, 0);

    public void AddPeer(Peer peer)
    {
        _peers[peer.ID] = peer;
        Interlocked.Increment(ref _connectedPeerCount);
    }

    public bool TryGetPeer(uint peerId, out Peer peer)
    {
        return _peers.TryGetValue(peerId, out peer);
    }

    public bool RemovePeerState(uint peerId)
    {
        bool removed = _peers.Remove(peerId);
        _reassemblyBuffers.Remove(peerId);

        if (removed)
            Interlocked.Decrement(ref _connectedPeerCount);

        return removed;
    }

    public List<Peer> SnapshotPeers()
    {
        return [.. _peers.Values];
    }

    public void ClearAll()
    {
        _peers.Clear();
        _reassemblyBuffers.Clear();
    }

    public Dictionary<ushort, FragmentBuffer> GetOrCreateReassembly(uint peerId)
    {
        if (_reassemblyBuffers.TryGetValue(peerId, out Dictionary<ushort, FragmentBuffer>? buffers))
            return buffers;

        buffers = [];
        _reassemblyBuffers[peerId] = buffers;
        return buffers;
    }

    public void RemoveReassemblyStream(uint peerId, ushort streamId)
    {
        if (_reassemblyBuffers.TryGetValue(peerId, out Dictionary<ushort, FragmentBuffer>? buffers))
            buffers.Remove(streamId);
    }

    public void ClearReassembly()
    {
        _reassemblyBuffers.Clear();
    }
}
