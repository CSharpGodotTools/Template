using Framework.Netcode.Server;
using Godot;
using System.Collections.Generic;
using System.Diagnostics;

namespace Framework.Netcode.Examples.Topdown;

public partial class GameServer : GodotServer
{
    private const int PositionBroadcastIntervalMs = 50;

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
        BroadcastPositions(force: false, excludeId: peerId);
    }

    private void AddPlayer(uint peerId)
    {
        if (!_players.Add(peerId))
        {
            return;
        }

        // Tell the new player their own ID
        Send(new SPacketPlayerJoinedLeaved
        {
            Id = peerId,
            Joined = true,
            IsLocal = true
        }, peerId);

        // Tell everyone else about the new player
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
        {
            return;
        }

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
        Dictionary<uint, Vector2> snapshot = [];

        foreach (KeyValuePair<uint, Vector2> positionEntry in _positions)
        {
            if (positionEntry.Key != peerId)
            {
                snapshot[positionEntry.Key] = positionEntry.Value;
            }
        }

        Send(new SPacketPlayerPositions { Positions = snapshot }, peerId);
    }

    private void BroadcastPositions(bool force = false, uint excludeId = 0)
    {
        if (!CanBroadcastPositions(force))
        {
            return;
        }

        SPacketPlayerPositions packet = new()
        {
            Positions = new Dictionary<uint, Vector2>(_positions)
        };

        if (excludeId > 0)
        {
            Broadcast(packet, excludeId);
        }
        else
        {
            Broadcast(packet);
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

        long broadcastIntervalTicks = (long)(PositionBroadcastIntervalMs * (double)Stopwatch.Frequency / 1000.0);
        if (now - _lastPositionBroadcastTicks < broadcastIntervalTicks)
        {
            return false;
        }

        _lastPositionBroadcastTicks = now;
        return true;
    }
}
