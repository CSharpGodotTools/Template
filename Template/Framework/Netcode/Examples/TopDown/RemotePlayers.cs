using Godot;
using System.Collections.Generic;

namespace Framework.Netcode.Examples.Topdown;

internal sealed class RemotePlayers
{
    private const float RemoteLerpSpeed = 6f;

    private readonly World _world;
    private readonly Dictionary<uint, ColorRect> _players = [];
    private readonly Dictionary<uint, Vector2> _targetPositions = [];

    public RemotePlayers(World world)
    {
        _world = world;
    }

    public void EnsureRemote(uint id)
    {
        EnsurePlayerNode(id);
    }

    public void Remove(uint id)
    {
        if (_players.Remove(id, out ColorRect playerNode))
        {
            playerNode.QueueFree();
        }

        _targetPositions.Remove(id);
    }

    public void ClearAll()
    {
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
            ColorRect playerNode = EnsurePlayerNode(positionEntry.Key);
            if (!_targetPositions.ContainsKey(positionEntry.Key))
            {
                playerNode.Position = positionEntry.Value;
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
            if (_players.TryGetValue(positionEntry.Key, out ColorRect playerNode))
            {
                playerNode.Position = playerNode.Position.Lerp(positionEntry.Value, interpolation);
            }
        }
    }

    private ColorRect EnsurePlayerNode(uint id)
    {
        if (_players.TryGetValue(id, out ColorRect existingNode))
        {
            return existingNode;
        }

        ColorRect playerNode = World.CreatePlayerRect(new Color(1f, 0.55f, 0.2f));
        playerNode.Name = $"Player_{id}";
        playerNode.Position = _world.GetScreenCenter();
        _players[id] = playerNode;
        _world.AddChild(playerNode);
        return playerNode;
    }
}
