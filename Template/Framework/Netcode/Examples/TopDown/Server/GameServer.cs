using Framework.Netcode;
using Framework.Netcode.Server;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Netcode.Examples.Topdown;

public partial class GameServer : GodotServer
{
    private const int PositionBroadcastIntervalMs = 100;

    private readonly HashSet<uint> _players = [];
    private readonly Dictionary<uint, Vector2> _positions = [];

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
        UpdatePlayerPosition(peer.PeerId, peer.Packet.Position);
    }

    private void UpdatePlayerPosition(uint peerId, Vector2 position)
    {
        if (!_players.Contains(peerId))
            return;

        _positions[peerId] = position;
        BroadcastPositions();
    }

    private void AddPlayer(uint peerId)
    {
        if (!_players.Add(peerId))
            return;

        NotifySelfJoined(peerId);
        BroadcastPlayerJoin(peerId);
        SendExistingPlayersTo(peerId);
        SendPositionsSnapshotTo(peerId);
    }

    private void NotifySelfJoined(uint peerId)
    {
        Send(new SPacketPlayerJoinedLeaved
        {
            Id = peerId,
            Joined = true,
            IsLocal = true
        }, peerId);
    }

    private void BroadcastPlayerJoin(uint peerId)
    {
        Broadcast(new SPacketPlayerJoinedLeaved
        {
            Id = peerId,
            Joined = true,
            IsLocal = false
        }, peerId);
    }

    private void RemovePlayer(uint playerId)
    {
        if (!_players.Remove(playerId))
            return;

        _positions.Remove(playerId);
        BroadcastPlayerLeave(playerId);
        BroadcastPositions(force: true);
    }

    private void BroadcastPlayerLeave(uint playerId)
    {
        Broadcast(new SPacketPlayerJoinedLeaved { Id = playerId, Joined = false });
    }

    private void SendExistingPlayersTo(uint peerId)
    {
        Send(peerId, BuildJoinPackets(except: peerId));
    }

    /// <summary>
    /// Create a sequence of <see cref="SPacketPlayerJoinedLeaved"/> packets
    /// representing every player on the server except the supplied ID.
    /// </summary>
    private IEnumerable<ServerPacket> BuildJoinPackets(uint except)
    {
        foreach (uint playerId in _players)
        {
            if (playerId == except)
                continue;

            yield return new SPacketPlayerJoinedLeaved
            {
                Id = playerId,
                Joined = true,
                IsLocal = false
            };
        }
    }

    private void SendPositionsSnapshotTo(uint peerId)
    {
        // Pass _positions directly — Write() serialises it synchronously before returning.
        Send(new SPacketPlayerPositions { Positions = _positions }, peerId);
    }

    private void BroadcastPositions(bool force = false)
    {
        if (_players.Count == 0)
            return;

        // throttle the payload automatically by packet type; helper returns true
        // iff the packet was enqueued (return value ignored).
        SendThrottled(
            new SPacketPlayerPositions { Positions = _positions },
            _players,
            PositionBroadcastIntervalMs,
            force);
    }
}
