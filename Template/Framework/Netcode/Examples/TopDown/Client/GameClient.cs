using Framework.Netcode.Client;
using Godot;
using System;
using System.Collections.Generic;

namespace Framework.Netcode.Examples.Topdown;

public partial class GameClient : GodotClient
{
    private readonly Dictionary<uint, Vector2> _remotePositions = [];
    private uint? _localPlayerId;

    public GameClient()
    {
        OnPacket<SPacketPlayerJoinedLeaved>(OnPlayerJoinedLeaved);
        OnPacket<SPacketPlayerPositions>(OnPlayerPositions);
    }

    public event Action LocalPlayerReady;
    public event Action<uint> RemotePlayerJoined;
    public event Action<uint> RemotePlayerLeft;
    public event Action<IReadOnlyDictionary<uint, Vector2>> RemotePositionsUpdated;

    protected override void OnConnected()
    {
        Send(new CPacketPlayerJoinLeave { Joined = true });
    }

    protected override void OnDisconnected()
    {
        _localPlayerId = null;
        _remotePositions.Clear();
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
        if (!_localPlayerId.HasValue)
            return;

        ApplyRemotePositions(packet.Positions);
    }

    private void HandlePlayerJoined(SPacketPlayerJoinedLeaved packet)
    {
        if (packet.IsLocal)
        {
            _localPlayerId = packet.Id;
            LocalPlayerReady?.Invoke();
            return;
        }

        if (packet.Id == _localPlayerId)
            return;

        RemotePlayerJoined?.Invoke(packet.Id);
    }

    private void HandlePlayerLeft(uint id)
    {
        if (id == _localPlayerId)
            return;

        _remotePositions.Remove(id);
        RemotePlayerLeft?.Invoke(id);
    }

    private void ApplyRemotePositions(IReadOnlyDictionary<uint, Vector2> positions)
    {
        // Skip all work when nothing is listening — the common case for bots.
        if (RemotePositionsUpdated == null)
            return;

        uint localId = _localPlayerId!.Value;
        _remotePositions.Clear();

        foreach (KeyValuePair<uint, Vector2> entry in positions)
        {
            if (entry.Key != localId)
            {
                _remotePositions[entry.Key] = entry.Value;
            }
        }

        // Pass the internal dictionary directly — callers must not retain the reference.
        RemotePositionsUpdated.Invoke(_remotePositions);
    }
}
