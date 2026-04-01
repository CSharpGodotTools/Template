using Godot;
using System.Collections.Generic;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// Tracks remote-player nodes, target positions, and interpolation updates.
/// </summary>
internal sealed class RemotePlayers
{
    private const float RemoteLerpSpeed = 6f;

    private readonly World _world;
    private readonly HashSet<uint> _trackedIds = [];
    private readonly Dictionary<uint, ColorRect> _players = [];
    private readonly Dictionary<uint, Vector2> _targetPositions = [];

    /// <summary>
    /// Creates remote-player controller for a world instance.
    /// </summary>
    /// <param name="world">TopDown world host node.</param>
    public RemotePlayers(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Ensures a remote player id is tracked and eligible for position updates.
    /// </summary>
    /// <param name="id">Remote player id.</param>
    public void EnsureRemote(uint id)
    {
        _trackedIds.Add(id);
    }

    /// <summary>
    /// Removes a remote player and frees its visual node.
    /// </summary>
    /// <param name="id">Remote player id.</param>
    public void Remove(uint id)
    {
        _trackedIds.Remove(id);

        // Free the visual node only when one exists for this player id.
        if (_players.Remove(id, out ColorRect? playerNode))
        {
            playerNode.QueueFree();
        }

        _targetPositions.Remove(id);
    }

    /// <summary>
    /// Clears all tracked remote players and visual nodes.
    /// </summary>
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

    /// <summary>
    /// Updates remote target positions and lazily spawns missing remote nodes.
    /// </summary>
    /// <param name="positions">Latest position snapshot keyed by player id.</param>
    public void UpdateTargets(IReadOnlyDictionary<uint, Vector2> positions)
    {
        foreach (KeyValuePair<uint, Vector2> positionEntry in positions)
        {
            // Ignore updates for ids that are not currently tracked as remotes.
            if (!_trackedIds.Contains(positionEntry.Key))
                continue;

            // Spawn visual node on first position update for each tracked remote id.
            if (!_players.ContainsKey(positionEntry.Key))
            {
                ColorRect playerNode = World.CreatePlayerRect(new Color(1f, 0.55f, 0.2f));
                playerNode.Name = $"Player_{positionEntry.Key}";
                playerNode.Position = positionEntry.Value;
                _players[positionEntry.Key] = playerNode;
                _world.AddChild(playerNode);
            }

            _targetPositions[positionEntry.Key] = positionEntry.Value;
        }
    }

    /// <summary>
    /// Interpolates remote-player nodes toward latest target positions.
    /// </summary>
    /// <param name="deltaSeconds">Frame delta in seconds.</param>
    public void Tick(float deltaSeconds)
    {
        // Skip interpolation work when there are no remote targets.
        if (_targetPositions.Count == 0)
        {
            return;
        }

        // Exponential interpolation keeps motion smooth regardless of frame rate.
        float interpolation = 1f - Mathf.Exp(-RemoteLerpSpeed * deltaSeconds);

        foreach (KeyValuePair<uint, Vector2> positionEntry in _targetPositions)
        {
            // Interpolate only nodes that are still present in the scene.
            if (_players.TryGetValue(positionEntry.Key, out ColorRect? playerNode))
            {
                playerNode.Position = playerNode.Position.Lerp(positionEntry.Value, interpolation);
            }
        }
    }
}
