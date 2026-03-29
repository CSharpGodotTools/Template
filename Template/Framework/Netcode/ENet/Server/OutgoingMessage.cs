namespace __TEMPLATE__.Netcode.Server;

internal sealed class OutgoingMessage
{
    private readonly byte[] _data;
    private readonly bool _isBroadcast;
    private readonly uint _targetPeerId;
    private readonly uint _excludePeerId;
    private readonly bool _hasExclusion;

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

    public byte[] Data => _data;
    public bool IsBroadcast => _isBroadcast;
    public uint TargetPeerId => _targetPeerId;
    public uint ExcludePeerId => _excludePeerId;
    public bool HasExclusion => _hasExclusion;

    public static OutgoingMessage Unicast(byte[] data, uint peerId)
    {
        return new OutgoingMessage(data, false, peerId, 0, false);
    }

    public static OutgoingMessage Broadcast(byte[] data)
    {
        return new OutgoingMessage(data, true, 0, 0, false);
    }

    public static OutgoingMessage BroadcastExcept(byte[] data, uint excludeId)
    {
        return new OutgoingMessage(data, true, 0, excludeId, true);
    }
}
