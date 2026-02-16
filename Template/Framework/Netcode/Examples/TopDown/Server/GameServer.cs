using ENet;
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

    public GameServer()
    {
        RegisterPacketHandler<CPacketPlayerJoinLeave>(OnPlayerJoinLeave);
        RegisterPacketHandler<CPacketPlayerPosition>(OnPlayerPosition);
    }

    protected override void OnPeerDisconnect(Event netEvent)
    {
        RemovePlayer(netEvent.Peer.ID);
    }

    private void OnPlayerJoinLeave(CPacketPlayerJoinLeave packet, Peer peer)
    {
        if (packet.Joined)
        {
            AddPlayer(peer);
        }
        else
        {
            RemovePlayer(peer.ID);
        }
    }

    private void OnPlayerPosition(CPacketPlayerPosition packet, Peer peer)
    {
        if (!_players.Contains(peer.ID))
        {
            return;
        }

        _positions[peer.ID] = packet.Position;
        BroadcastPositions(force: false, excludedPeer: peer);
    }

    private void AddPlayer(Peer peer)
    {
        if (!_players.Add(peer.ID))
        {
            return;
        }

        Send(new SPacketPlayerJoinedLeaved
        {
            Id = peer.ID,
            Joined = true,
            IsLocal = true
        }, peer);

        Broadcast(new SPacketPlayerJoinedLeaved
        {
            Id = peer.ID,
            Joined = true,
            IsLocal = false
        }, peer);

        SendExistingPlayersTo(peer);
        SendPositionsSnapshotTo(peer);
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

    private void SendExistingPlayersTo(Peer peer)
    {
        foreach (uint playerId in _players)
        {
            if (playerId != peer.ID)
            {
                Send(new SPacketPlayerJoinedLeaved
                {
                    Id = playerId,
                    Joined = true,
                    IsLocal = false
                }, peer);
            }
        }
    }

    private void SendPositionsSnapshotTo(Peer peer)
    {
        Dictionary<uint, Vector2> snapshot = [];

        foreach (KeyValuePair<uint, Vector2> positionEntry in _positions)
        {
            if (positionEntry.Key != peer.ID)
            {
                snapshot[positionEntry.Key] = positionEntry.Value;
            }
        }

        Send(new SPacketPlayerPositions { Positions = snapshot }, peer);
    }

    private void BroadcastPositions(bool force = false, Peer? excludedPeer = null)
    {
        if (!CanBroadcastPositions(force))
        {
            return;
        }

        SPacketPlayerPositions packet = new()
        {
            Positions = new Dictionary<uint, Vector2>(_positions)
        };

        if (excludedPeer.HasValue)
        {
            Broadcast(packet, excludedPeer.Value);
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
