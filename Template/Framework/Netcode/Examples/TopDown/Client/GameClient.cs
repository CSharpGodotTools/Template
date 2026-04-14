using __TEMPLATE__.Netcode.Client;
using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// TopDown sample client that tracks local/remote player state and relays position updates.
/// </summary>
public partial class GameClient : GodotClient
{
    private readonly Dictionary<uint, Vector2> _remotePositions = [];
    private uint? _localPlayerId;

    /// <summary>
    /// Creates the sample client and registers packet handlers.
    /// </summary>
    public GameClient()
    {
        OnPacket<SPacketPlayerJoinedLeaved>(OnPlayerJoinedLeaved);
        OnPacket<SPacketPlayerPositions>(OnPlayerPositions);
    }

    /// <summary>
    /// Raised when local player identity has been established.
    /// </summary>
    public event Action? LocalPlayerReady;

    /// <summary>
    /// Raised when a remote player joins.
    /// </summary>
    public event Action<uint>? RemotePlayerJoined;

    /// <summary>
    /// Raised when a remote player leaves.
    /// </summary>
    public event Action<uint>? RemotePlayerLeft;

    /// <summary>
    /// Raised when remote target positions have been updated.
    /// </summary>
    public event Action<IReadOnlyDictionary<uint, Vector2>>? RemotePositionsUpdated;

    /// <summary>
    /// Announces local join to the server once connected.
    /// </summary>
    protected override void OnConnected()
    {
        Send(new CPacketPlayerJoinLeave { Joined = true });
    }

    /// <summary>
    /// Clears local session state after disconnect.
    /// </summary>
    protected override void OnDisconnected()
    {
        _localPlayerId = null;
        _remotePositions.Clear();
    }

    /// <summary>
    /// Sends local player position to the server.
    /// </summary>
    /// <param name="position">Current local player position.</param>
    public void SendPosition(Vector2 position)
    {
        Send(new CPacketPlayerPosition { Position = position });
    }

    /// <summary>
    /// Handles player join/leave lifecycle packets.
    /// </summary>
    /// <param name="packet">Join/leave packet payload.</param>
    private void OnPlayerJoinedLeaved(SPacketPlayerJoinedLeaved packet)
    {
        // Route packets to join or leave handlers based on server intent.
        if (packet.Joined)
        {
            HandlePlayerJoined(packet);
        }
        else
        {
            HandlePlayerLeft(packet.Id);
        }
    }

    /// <summary>
    /// Handles incoming remote position snapshots.
    /// </summary>
    /// <param name="packet">Position snapshot packet payload.</param>
    private void OnPlayerPositions(SPacketPlayerPositions packet)
    {
        // Ignore snapshots until the server has assigned a local player id.
        if (!_localPlayerId.HasValue)
            return;

        ApplyRemotePositions(packet.Positions);
    }

    /// <summary>
    /// Handles player-joined notifications and distinguishes local vs remote joins.
    /// </summary>
    /// <param name="packet">Join packet payload.</param>
    private void HandlePlayerJoined(SPacketPlayerJoinedLeaved packet)
    {
        // Local join initializes identity and notifies startup listeners.
        if (packet.IsLocal)
        {
            _localPlayerId = packet.Id;
            LocalPlayerReady?.Invoke();
            return;
        }

        // Ignore accidental echo events for our own player id.
        if (packet.Id == _localPlayerId)
            return;

        RemotePlayerJoined?.Invoke(packet.Id);
    }

    /// <summary>
    /// Handles player-left notifications.
    /// </summary>
    /// <param name="id">Leaving player id.</param>
    private void HandlePlayerLeft(uint id)
    {
        // Ignore local leave events here because disconnect handles local cleanup.
        if (id == _localPlayerId)
            return;

        _remotePositions.Remove(id);
        RemotePlayerLeft?.Invoke(id);
    }

    /// <summary>
    /// Applies remote position snapshot data and notifies listeners.
    /// </summary>
    /// <param name="positions">Latest known position map keyed by player id.</param>
    private void ApplyRemotePositions(IReadOnlyDictionary<uint, Vector2> positions)
    {
        // Skip all work when nothing is listening — the common case for bots.
        if (RemotePositionsUpdated == null)
            return;

        uint localId = _localPlayerId!.Value;
        _remotePositions.Clear();

        foreach (KeyValuePair<uint, Vector2> entry in positions)
        {
            // Keep only remote peers in the remote position cache.
            if (entry.Key != localId)
                _remotePositions[entry.Key] = entry.Value;
        }

        // Pass the internal dictionary directly — callers must not retain the reference.
        RemotePositionsUpdated.Invoke(_remotePositions);
    }
}
