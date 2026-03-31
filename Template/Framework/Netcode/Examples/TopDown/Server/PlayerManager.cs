using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Netcode.Examples.Topdown.Server;

/// <summary>
/// Tracks the set of connected players and their latest positions.
/// Raises events when players join, leave, or move.
/// </summary>
public class PlayerManager
{
    private readonly HashSet<uint> _players = [];
    private readonly Dictionary<uint, Vector2> _positions = [];

    /// <summary>
    /// Fired when a player is added successfully (not called if already present).
    /// </summary>
    public event Action<uint>? PlayerJoined;

    /// <summary>
    /// Fired when a player is removed successfully (not called if not present).
    /// </summary>
    public event Action<uint>? PlayerLeft;

    /// <summary>
    /// Fired whenever an existing player's position is updated.
    /// </summary>
    public event Action<uint, Vector2>? PositionUpdated;

    /// <summary>
    /// Adds a player ID. Returns true if the player was new.
    /// </summary>
    public bool Add(uint playerId)
    {
        if (_players.Add(playerId))
        {
            PlayerJoined?.Invoke(playerId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes a player ID. Returns true if the player was present.
    /// </summary>
    public bool Remove(uint playerId)
    {
        if (_players.Remove(playerId))
        {
            _positions.Remove(playerId);
            PlayerLeft?.Invoke(playerId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Update a player's position. Ignored if the player isn't tracked.
    /// </summary>
    public bool UpdatePosition(uint playerId, Vector2 position)
    {
        if (!_players.Contains(playerId))
            return false;

        _positions[playerId] = position;
        PositionUpdated?.Invoke(playerId, position);
        return true;
    }

    /// <summary>
    /// Enumerates all tracked player IDs, optionally excluding one.
    /// </summary>
    public IEnumerable<uint> ExistingPlayersExcept(uint? except = null)
    {
        return except.HasValue ? _players.Where(id => id != except.Value) : _players;
    }

    /// <summary>
    /// Build the join notification packets for every player except the given
    /// ID.
    /// </summary>
    public IEnumerable<ServerPacket> BuildJoinPackets(uint except)
    {
        foreach (uint playerId in ExistingPlayersExcept(except))
        {
            yield return new SPacketPlayerJoinedLeaved
            {
                Id = playerId,
                Joined = true,
                IsLocal = false
            };
        }
    }

    /// <summary>
    /// Exposes the live positions dictionary for read-only access.
    /// </summary>
    public IReadOnlyDictionary<uint, Vector2> Positions => _positions;

    /// <summary>
    /// Number of currently tracked players.
    /// </summary>
    public int Count => _players.Count;
}
