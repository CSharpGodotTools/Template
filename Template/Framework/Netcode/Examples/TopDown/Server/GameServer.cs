using Framework.Netcode;
using Framework.Netcode.Server;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Netcode.Examples.Topdown.Server;

namespace Framework.Netcode.Examples.Topdown;

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


    private void BroadcastPlayerLeave(uint playerId)
    {
        Broadcast(new SPacketPlayerJoinedLeaved { Id = playerId, Joined = false });
    }

    private void SendExistingPlayersTo(uint peerId)
    {
        Send(peerId, _manager.BuildJoinPackets(peerId));
    }


    private void SendPositionsSnapshotTo(uint peerId)
    {
        // Pass positions dictionary from player manager.
        Send(new SPacketPlayerPositions { Positions = new Dictionary<uint, Vector2>(_manager.Positions) }, peerId);
    }

    private void BroadcastPositions(bool force = false)
    {
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
