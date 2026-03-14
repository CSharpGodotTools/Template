using Godot;
using System.Collections.Generic;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

internal sealed class RemotePlayers
{
    private const float RemoteLerpSpeed = 6f;

    private readonly World _world;
    private readonly HashSet<uint> _trackedIds = [];
    private readonly Dictionary<uint, ColorRect> _players = [];
    private readonly Dictionary<uint, Vector2> _targetPositions = [];

    public RemotePlayers(World world)
    {
        _world = world;
    }

    public void EnsureRemote(uint id)
    {
        _trackedIds.Add(id);
    }

    public void Remove(uint id)
    {
        _trackedIds.Remove(id);

        if (_players.Remove(id, out ColorRect? playerNode))
        {
            playerNode.QueueFree();
        }

        _targetPositions.Remove(id);
    }

    public void ClearAll()
    {
        _trackedIds.Clear();

        foreach (ColorRect playerNode in _players.Values)
        {
            playerNode.QueueFree();
        }

        _players.Clear();
        _targetPositions.Clear();
    }

    public void UpdateTargets(IReadOnlyDictionary<uint, Vector2> positions)
    {
        foreach (KeyValuePair<uint, Vector2> positionEntry in positions)
        {
            if (!_trackedIds.Contains(positionEntry.Key))
                continue;

            if (!_players.TryGetValue(positionEntry.Key, out ColorRect? playerNode))
            {
                playerNode = World.CreatePlayerRect(new Color(1f, 0.55f, 0.2f));
                playerNode.Name = $"Player_{positionEntry.Key}";
                playerNode.Position = positionEntry.Value;
                _players[positionEntry.Key] = playerNode;
                _world.AddChild(playerNode);
            }

            _targetPositions[positionEntry.Key] = positionEntry.Value;
        }
    }

    public void Tick(float deltaSeconds)
    {
        if (_targetPositions.Count == 0)
        {
            return;
        }

        float interpolation = 1f - Mathf.Exp(-RemoteLerpSpeed * deltaSeconds);

        foreach (KeyValuePair<uint, Vector2> positionEntry in _targetPositions)
        {
            if (_players.TryGetValue(positionEntry.Key, out ColorRect? playerNode))
            {
                playerNode.Position = playerNode.Position.Lerp(positionEntry.Value, interpolation);
            }
        }
    }
}

