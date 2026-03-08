using Framework.Netcode.Server;
using Godot;
using System.Collections.Generic;
using System.Diagnostics;

namespace Framework.Netcode.Examples.Topdown;

public partial class GameServer : GodotServer
{
    // Clients that opt in to receiving position broadcasts, typically human players only.
    // Bots never subscribe, which eliminates the majority of outgoing position traffic.
    private const int PositionBroadcastIntervalMs = 100;

    // Precompute tick interval once to avoid per-call multiplication.
    private static readonly long _broadcastIntervalTicks =
        (long)(PositionBroadcastIntervalMs * (double)Stopwatch.Frequency / 1000.0);

    private readonly HashSet<uint> _players = [];
    private readonly HashSet<uint> _positionSubscribers = [];
    private readonly Dictionary<uint, Vector2> _positions = [];
    private long _lastPositionBroadcastTicks;

    protected override void RegisterPackets()
    {
        OnPacket<CPacketPlayerJoinLeave>(OnPlayerJoinLeave);
        OnPacket<CPacketPlayerPosition>(OnPlayerPosition);
        OnPacket<CPacketSubscribePositions>(OnSubscribePositions);
    }

    protected override void OnPeerDisconnected(uint peerId)
    {
        RemovePlayer(peerId);
    }

    private void OnPlayerJoinLeave(CPacketPlayerJoinLeave packet, uint peerId)
    {
        if (packet.Joined)
        {
            AddPlayer(peerId);
        }
        else
        {
            RemovePlayer(peerId);
        }
    }

    private void OnPlayerPosition(CPacketPlayerPosition packet, uint peerId)
    {
        if (!_players.Contains(peerId))
        {
            return;
        }

        _positions[peerId] = packet.Position;
        BroadcastPositions();
    }

    private void OnSubscribePositions(CPacketSubscribePositions packet, uint peerId)
    {
        if (!_players.Contains(peerId) || !_positionSubscribers.Add(peerId))
        {
            return;
        }

        // Send the current snapshot so the subscriber is immediately up to date.
        SendPositionsSnapshotTo(peerId);
    }

    private void AddPlayer(uint peerId)
    {
        if (!_players.Add(peerId))
        {
            return;
        }

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
        // Position snapshot is deferred until the client sends CPacketSubscribePositions.
    }

    private void RemovePlayer(uint playerId)
    {
        if (!_players.Remove(playerId))
        {
            return;
        }

        _positions.Remove(playerId);
        _positionSubscribers.Remove(playerId);
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
        if (!CanBroadcastPositions(force) || _positionSubscribers.Count == 0)
        {
            return;
        }

        // Serialise once and enqueue a unicast per subscriber to avoid redundant copies.
        // The core transport will fragment automatically if the payload exceeds MaxSize.
        SPacketPlayerPositions packet = new() { Positions = _positions };
        packet.Write();
        byte[] data = packet.GetData();

        foreach (uint subscriberId in _positionSubscribers)
        {
            EnqueueOutgoing(OutgoingMessage.Unicast(data, subscriberId));
        }
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
