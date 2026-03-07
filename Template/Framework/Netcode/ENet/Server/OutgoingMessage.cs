namespace Framework.Netcode.Server;

public abstract partial class ENetServer
{
    /// <summary>
    /// Describes unit of work queued for the server worker thread.
    /// </summary>
    protected readonly struct OutgoingMessage
    {
        public byte[] Data { get; }
        public bool IsBroadcast { get; }
        public uint TargetPeerId { get; }
        public uint ExcludePeerId { get; }
        public bool HasExclusion { get; }

        private OutgoingMessage(
            byte[] data, bool isBroadcast,
            uint targetPeerId, uint excludePeerId, bool hasExclusion)
        {
            Data = data;
            IsBroadcast = isBroadcast;
            TargetPeerId = targetPeerId;
            ExcludePeerId = excludePeerId;
            HasExclusion = hasExclusion;
        }

        /// <summary>
        /// Creates a message targeting a single peer.
        /// </summary>
        public static OutgoingMessage Unicast(byte[] data, uint peerId)
        {
            return new OutgoingMessage(data, false, peerId, 0, false);
        }

        /// <summary>
        /// Creates a broadcast message to all connected peers.
        /// </summary>
        public static OutgoingMessage Broadcast(byte[] data)
        {
            return new OutgoingMessage(data, true, 0, 0, false);
        }

        /// <summary>
        /// Creates a broadcast message excluding one peer.
        /// </summary>
        public static OutgoingMessage BroadcastExcept(byte[] data, uint excludeId)
        {
            return new OutgoingMessage(data, true, 0, excludeId, true);
        }
    }
}
