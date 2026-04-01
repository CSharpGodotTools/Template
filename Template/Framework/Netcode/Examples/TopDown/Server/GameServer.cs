using __TEMPLATE__.Netcode.Examples.Topdown.Server;
using __TEMPLATE__.Netcode.Server;
using Godot;
using System.Collections.Generic;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// TopDown sample server that tracks players and broadcasts join, leave, and position updates.
/// </summary>
public partial class GameServer : GodotServer
{
    private const int PositionBroadcastIntervalMs = 100;

    private readonly PlayerManager _manager = new();

    protected override void OnPeerDisconnected(uint peerId)
    {
        _manager.Remove(peerId);
    }

    public GameServer()
    {
        // packet registration moved here from base class hook
        OnPacket<CPacketPlayerJoinLeave>(peer =>
        {
            // Add or remove players based on join/leave packet intent.
            if (peer.Packet.Joined)
                _manager.Add(peer.PeerId);
            else
                _manager.Remove(peer.PeerId);
        });

        OnPacket<CPacketPlayerPosition>(peer =>
            _manager.UpdatePosition(peer.PeerId, peer.Packet.Position));

        _manager.PlayerJoined += peerId =>
        {
            NotifySelfJoined(peerId);
            BroadcastPlayerJoin(peerId);
            SendExistingPlayersTo(peerId);
            SendPositionsSnapshotTo(peerId);
        };

        _manager.PlayerLeft += peerId =>
        {
            BroadcastPlayerLeave(peerId);
            BroadcastPositions(force: true);
        };

        _manager.PositionUpdated += (_, _) => BroadcastPositions();
    }

    /// <summary>
    /// Sends a self-join packet to the newly connected peer so it can identify its local player id.
    /// </summary>
    /// <param name="peerId">Connected peer id.</param>
    private void NotifySelfJoined(uint peerId)
    {
        Send(new SPacketPlayerJoinedLeaved
        {
            Id = peerId,
            Joined = true,
            IsLocal = true
        }, peerId);
    }

    /// <summary>
    /// Broadcasts a remote-player join packet to all peers except the joining peer.
    /// </summary>
    /// <param name="peerId">Joining peer id.</param>
    private void BroadcastPlayerJoin(uint peerId)
    {
        Broadcast(new SPacketPlayerJoinedLeaved
        {
            Id = peerId,
            Joined = true,
            IsLocal = false
        }, peerId);
    }

    /// <summary>
    /// Broadcasts a player-leave packet to connected peers.
    /// </summary>
    /// <param name="playerId">Peer id that left the session.</param>
    private void BroadcastPlayerLeave(uint playerId)
    {
        Broadcast(new SPacketPlayerJoinedLeaved { Id = playerId, Joined = false });
    }

    /// <summary>
    /// Sends existing-player join packets to a newly connected peer.
    /// </summary>
    /// <param name="peerId">Target peer id.</param>
    private void SendExistingPlayersTo(uint peerId)
    {
        Send(peerId, _manager.BuildJoinPackets(peerId));
    }

    /// <summary>
    /// Sends the latest position map snapshot to a specific peer.
    /// </summary>
    /// <param name="peerId">Target peer id.</param>
    private void SendPositionsSnapshotTo(uint peerId)
    {
        // Pass positions dictionary from player manager.
        Send(new SPacketPlayerPositions { Positions = new Dictionary<uint, Vector2>(_manager.Positions) }, peerId);
    }

    /// <summary>
    /// Broadcasts position updates to connected peers, honoring throttle rules unless forced.
    /// </summary>
    /// <param name="force">True to bypass throttling for immediate delivery.</param>
    private void BroadcastPositions(bool force = false)
    {
        // Skip broadcasts when no players are currently tracked.
        if (_manager.Count == 0)
            return;

        // throttle the payload automatically by packet type; helper returns true
        // iff the packet was enqueued (return value ignored).
        SendThrottled(
            new SPacketPlayerPositions { Positions = new Dictionary<uint, Vector2>(_manager.Positions) },
            _manager.ExistingPlayersExcept(null),
            PositionBroadcastIntervalMs,
            force);
    }
}
