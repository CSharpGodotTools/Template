namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Immutable send envelope describing unicast or broadcast payload routing.
/// </summary>
internal sealed class OutgoingMessage
{
    private readonly byte[] _data;
    private readonly bool _isBroadcast;
    private readonly uint _targetPeerId;
    private readonly uint _excludePeerId;
    private readonly bool _hasExclusion;

    /// <summary>
    /// Creates an outgoing message envelope.
    /// </summary>
    /// <param name="data">Serialized payload bytes.</param>
    /// <param name="isBroadcast">Whether this message targets broadcast delivery.</param>
    /// <param name="targetPeerId">Unicast target peer id.</param>
    /// <param name="excludePeerId">Broadcast exclusion peer id.</param>
    /// <param name="hasExclusion">Whether exclusion should be applied during broadcast.</param>
    private OutgoingMessage(
        byte[] data,
        bool isBroadcast,
        uint targetPeerId,
        uint excludePeerId,
        bool hasExclusion)
    {
        _data = data;
        _isBroadcast = isBroadcast;
        _targetPeerId = targetPeerId;
        _excludePeerId = excludePeerId;
        _hasExclusion = hasExclusion;
    }

    /// <summary>
    /// Gets serialized payload bytes.
    /// </summary>
    public byte[] Data => _data;

    /// <summary>
    /// Gets whether this message is broadcast.
    /// </summary>
    public bool IsBroadcast => _isBroadcast;

    /// <summary>
    /// Gets target peer id for unicast messages.
    /// </summary>
    public uint TargetPeerId => _targetPeerId;

    /// <summary>
    /// Gets excluded peer id for broadcast-except messages.
    /// </summary>
    public uint ExcludePeerId => _excludePeerId;

    /// <summary>
    /// Gets whether broadcast exclusion is enabled.
    /// </summary>
    public bool HasExclusion => _hasExclusion;

    /// <summary>
    /// Creates a unicast message targeting one peer.
    /// </summary>
    /// <param name="data">Serialized payload bytes.</param>
    /// <param name="peerId">Target peer id.</param>
    /// <returns>Outgoing unicast envelope.</returns>
    public static OutgoingMessage Unicast(byte[] data, uint peerId)
    {
        return new OutgoingMessage(data, false, peerId, 0, false);
    }

    /// <summary>
    /// Creates a broadcast message for all peers.
    /// </summary>
    /// <param name="data">Serialized payload bytes.</param>
    /// <returns>Outgoing broadcast envelope.</returns>
    public static OutgoingMessage Broadcast(byte[] data)
    {
        return new OutgoingMessage(data, true, 0, 0, false);
    }

    /// <summary>
    /// Creates a broadcast message that excludes one peer.
    /// </summary>
    /// <param name="data">Serialized payload bytes.</param>
    /// <param name="excludeId">Peer id to exclude from broadcast delivery.</param>
    /// <returns>Outgoing broadcast envelope with exclusion.</returns>
    public static OutgoingMessage BroadcastExcept(byte[] data, uint excludeId)
    {
        return new OutgoingMessage(data, true, 0, excludeId, true);
    }
}
