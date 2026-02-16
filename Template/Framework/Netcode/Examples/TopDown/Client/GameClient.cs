using ENet;
using Framework.Netcode.Client;
using Godot;
using System;
using System.Collections.Generic;

namespace Framework.Netcode.Examples.Topdown;

public partial class GameClient : GodotClient
{
    private readonly Dictionary<uint, Vector2> _remotePositions = [];
    private readonly Dictionary<uint, Vector2> _pendingPositions = [];
    private uint _localId;
    private bool _hasLocalId;

    public event Action<uint> LocalPlayerReady;
    public event Action<uint> RemotePlayerJoined;
    public event Action<uint> RemotePlayerLeft;
    public event Action<IReadOnlyDictionary<uint, Vector2>> RemotePositionsUpdated;

    public bool HasLocalId => _hasLocalId;
    public uint LocalId => _localId;

    public GameClient()
    {
        RegisterPacketHandler<SPacketPlayerJoinedLeaved>(OnPlayerJoinedLeaved);
        RegisterPacketHandler<SPacketPlayerPositions>(OnPlayerPositions);
    }

    protected override void OnConnect(Event netEvent)
    {
        Send(new CPacketPlayerJoinLeave { Joined = true });
    }

    protected override void OnDisconnect(Event netEvent)
    {
        ResetLocalIdentity();
        _remotePositions.Clear();
        _pendingPositions.Clear();
    }

    public void SendPosition(Vector2 position)
    {
        Send(new CPacketPlayerPosition { Position = position });
    }

    private void OnPlayerJoinedLeaved(SPacketPlayerJoinedLeaved packet)
    {
        if (packet.Joined)
        {
            HandlePlayerJoined(packet);
        }
        else
        {
            HandlePlayerLeft(packet.Id);
        }
    }

    private void OnPlayerPositions(SPacketPlayerPositions packet)
    {
        if (!HasLocalId)
        {
            CachePendingPositions(packet.Positions);
            return;
        }

        ApplyRemotePositions(packet.Positions);
    }

    private void HandlePlayerJoined(SPacketPlayerJoinedLeaved packet)
    {
        if (packet.IsLocal)
        {
            HandleLocalPlayerJoined(packet.Id);
            return;
        }

        if (IsLocalPlayer(packet.Id))
        {
            return;
        }

        RemotePlayerJoined?.Invoke(packet.Id);
    }

    private void HandleLocalPlayerJoined(uint id)
    {
        if (!TrySetLocalIdentity(id))
        {
            return;
        }

        LocalPlayerReady?.Invoke(LocalId);
        FlushPendingPositions();
    }

    private void HandlePlayerLeft(uint id)
    {
        if (IsLocalPlayer(id))
        {
            return;
        }

        _remotePositions.Remove(id);
        _pendingPositions.Remove(id);
        RemotePlayerLeft?.Invoke(id);
    }

    private void CachePendingPositions(IReadOnlyDictionary<uint, Vector2> positions)
    {
        _pendingPositions.Clear();

        foreach (KeyValuePair<uint, Vector2> entry in positions)
        {
            _pendingPositions[entry.Key] = entry.Value;
        }
    }

    private void FlushPendingPositions()
    {
        if (_pendingPositions.Count == 0)
        {
            return;
        }

        ApplyRemotePositions(_pendingPositions);
        _pendingPositions.Clear();
    }

    private void ApplyRemotePositions(IReadOnlyDictionary<uint, Vector2> positions)
    {
        uint localId = LocalId;
        _remotePositions.Clear();

        foreach (KeyValuePair<uint, Vector2> entry in positions)
        {
            if (entry.Key != localId)
            {
                _remotePositions[entry.Key] = entry.Value;
            }
        }

        Dictionary<uint, Vector2> updatedPositions = new(_remotePositions);
        RemotePositionsUpdated?.Invoke(updatedPositions);
    }

    private bool IsLocalPlayer(uint playerId)
    {
        return HasLocalId && playerId == LocalId;
    }

    private bool TrySetLocalIdentity(uint localId)
    {
        bool hasChanged = !_hasLocalId || _localId != localId;
        _localId = localId;
        _hasLocalId = true;
        return hasChanged;
    }

    private void ResetLocalIdentity()
    {
        _hasLocalId = false;
        _localId = 0;
    }
}
