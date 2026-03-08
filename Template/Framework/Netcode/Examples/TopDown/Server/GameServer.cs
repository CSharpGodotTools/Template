using Framework.Netcode;
using Framework.Netcode.Server;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Framework.Netcode.Examples.Topdown;

public partial class GameServer : GodotServer
{
    private const int PositionBroadcastIntervalMs = 100;

    // Precompute tick interval once to avoid per-call multiplication.
    private static readonly long _broadcastIntervalTicks =
        (long)(PositionBroadcastIntervalMs * (double)Stopwatch.Frequency / 1000.0);

    private readonly HashSet<uint> _players = [];
    private readonly Dictionary<uint, Vector2> _positions = [];
    private long _lastPositionBroadcastTicks;

    protected override void RegisterPackets()
    {
        OnPacket<CPacketPlayerJoinLeave>(OnPlayerJoinLeave);
        OnPacket<CPacketPlayerPosition>(OnPlayerPosition);
    }

    protected override void OnPeerDisconnected(uint peerId)
    {
        RemovePlayer(peerId);
    }

    private void OnPlayerJoinLeave(PacketFromPeer<CPacketPlayerJoinLeave> peer)
    {
        if (peer.Packet.Joined)
        {
            AddPlayer(peer.PeerId);
        }
        else
        {
            RemovePlayer(peer.PeerId);
        }
    }

    private void OnPlayerPosition(PacketFromPeer<CPacketPlayerPosition> peer)
    {
        if (!_players.Contains(peer.PeerId))
            return;

        _positions[peer.PeerId] = peer.Packet.Position;
        BroadcastPositions();
    }

    private void AddPlayer(uint peerId)
    {
        if (!_players.Add(peerId))
            return;

        // Tell the new player their own ID.
        Send(new SPacketPlayerJoinedLeaved
        {
            Id = peerId,
            Joined = true,
            IsLocal = true
        }, peerId);

        // Tell everyone else about the new player.
        Broadcast(new SPacketPlayerJoinedLeaved
        {
            Id = peerId,
            Joined = true,
            IsLocal = false
        }, peerId);

        SendExistingPlayersTo(peerId);
        SendPositionsSnapshotTo(peerId);
    }

    private void RemovePlayer(uint playerId)
    {
        if (!_players.Remove(playerId))
            return;

        _positions.Remove(playerId);
        Broadcast(new SPacketPlayerJoinedLeaved { Id = playerId, Joined = false });
        BroadcastPositions(force: true);
    }

    private void SendExistingPlayersTo(uint peerId)
    {
        foreach (uint playerId in _players)
        {
            if (playerId != peerId)
            {
                Send(new SPacketPlayerJoinedLeaved
                {
                    Id = playerId,
                    Joined = true,
                    IsLocal = false
                }, peerId);
            }
        }
    }

    private void SendPositionsSnapshotTo(uint peerId)
    {
        // Pass _positions directly — Write() serialises it synchronously before returning.
        Send(new SPacketPlayerPositions { Positions = _positions }, peerId);
    }

    private void BroadcastPositions(bool force = false)
    {
        if (!CanBroadcastPositions(force) || _players.Count == 0)
            return;

        // Use the new Send overload which handles the one‑time serialization
        // internally. This keeps the game‑writer's code simple while still
        // avoiding per‑peer allocations.
        Send(new SPacketPlayerPositions { Positions = _positions }, _players);
    }

    private bool CanBroadcastPositions(bool force)
    {
        long now = Stopwatch.GetTimestamp();
        if (force)
        {
            _lastPositionBroadcastTicks = now;
            return true;
        }

        if (_lastPositionBroadcastTicks == 0)
        {
            _lastPositionBroadcastTicks = now;
            return true;
        }

        if (now - _lastPositionBroadcastTicks < _broadcastIntervalTicks)
        {
            return false;
        }

        _lastPositionBroadcastTicks = now;
        return true;
    }
}
