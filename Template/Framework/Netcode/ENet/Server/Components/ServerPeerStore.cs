using ENet;
using System.Collections.Generic;
using System.Threading;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Stores connected peers and per-peer fragment reassembly buffers.
/// </summary>
internal sealed class ServerPeerStore
{
    private readonly Dictionary<uint, Peer> _peers = [];
    private readonly Dictionary<uint, Dictionary<ushort, FragmentBuffer>> _reassemblyBuffers = [];
    private int _connectedPeerCount;

    /// <summary>
    /// Gets the current connected peer count.
    /// </summary>
    public int ConnectedPeerCount => Interlocked.CompareExchange(ref _connectedPeerCount, 0, 0);

    /// <summary>
    /// Adds or replaces peer state for a connected peer.
    /// </summary>
    /// <param name="peer">Connected peer to track.</param>
    public void AddPeer(Peer peer)
    {
        _peers[peer.ID] = peer;
        Interlocked.Increment(ref _connectedPeerCount);
    }

    /// <summary>
    /// Attempts to resolve a tracked peer by id.
    /// </summary>
    /// <param name="peerId">Peer identifier.</param>
    /// <param name="peer">Resolved peer when found.</param>
    /// <returns><see langword="true"/> when peer exists in the store.</returns>
    public bool TryGetPeer(uint peerId, out Peer peer)
    {
        return _peers.TryGetValue(peerId, out peer);
    }

    /// <summary>
    /// Removes peer and reassembly state for a peer id.
    /// </summary>
    /// <param name="peerId">Peer identifier.</param>
    /// <returns><see langword="true"/> when a peer entry was removed.</returns>
    public bool RemovePeerState(uint peerId)
    {
        bool removed = _peers.Remove(peerId);
        _reassemblyBuffers.Remove(peerId);

        // Decrement connected count only when a peer entry was removed.
        if (removed)
            Interlocked.Decrement(ref _connectedPeerCount);

        return removed;
    }

    /// <summary>
    /// Returns a snapshot list of currently connected peers.
    /// </summary>
    /// <returns>Snapshot of peer handles.</returns>
    public List<Peer> SnapshotPeers()
    {
        return [.. _peers.Values];
    }

    /// <summary>
    /// Clears all peer and reassembly state.
    /// </summary>
    public void ClearAll()
    {
        _peers.Clear();
        _reassemblyBuffers.Clear();
    }

    /// <summary>
    /// Gets or creates fragment reassembly storage for a peer.
    /// </summary>
    /// <param name="peerId">Peer identifier.</param>
    /// <returns>Per-peer fragment buffer map keyed by stream id.</returns>
    public Dictionary<ushort, FragmentBuffer> GetOrCreateReassembly(uint peerId)
    {
        // Reuse an existing reassembly dictionary when one is already present.
        if (_reassemblyBuffers.TryGetValue(peerId, out Dictionary<ushort, FragmentBuffer>? buffers))
            return buffers;

        buffers = [];
        _reassemblyBuffers[peerId] = buffers;
        return buffers;
    }

    /// <summary>
    /// Removes a specific reassembly stream for a peer.
    /// </summary>
    /// <param name="peerId">Peer identifier.</param>
    /// <param name="streamId">Fragment stream identifier.</param>
    public void RemoveReassemblyStream(uint peerId, ushort streamId)
    {
        // Remove stream entries only when peer reassembly buffers exist.
        if (_reassemblyBuffers.TryGetValue(peerId, out Dictionary<ushort, FragmentBuffer>? buffers))
            buffers.Remove(streamId);
    }

    /// <summary>
    /// Clears all fragment reassembly buffers.
    /// </summary>
    public void ClearReassembly()
    {
        _reassemblyBuffers.Clear();
    }
}
