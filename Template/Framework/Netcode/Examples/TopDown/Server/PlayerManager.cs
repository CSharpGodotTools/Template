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
    /// <param name="playerId">Player id to add.</param>
    /// <returns><see langword="true"/> when player was newly added.</returns>
    public bool Add(uint playerId)
    {
        // Fire join events only when this player id is newly tracked.
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
    /// <param name="playerId">Player id to remove.</param>
    /// <returns><see langword="true"/> when player existed and was removed.</returns>
    public bool Remove(uint playerId)
    {
        // Fire leave events only when this player id existed in the set.
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
    /// <param name="playerId">Player id to update.</param>
    /// <param name="position">Latest player position.</param>
    /// <returns><see langword="true"/> when position was updated.</returns>
    public bool UpdatePosition(uint playerId, Vector2 position)
    {
        // Ignore updates for players that are not currently tracked.
        if (!_players.Contains(playerId))
            return false;

        _positions[playerId] = position;
        PositionUpdated?.Invoke(playerId, position);
        return true;
    }

    /// <summary>
    /// Enumerates all tracked player IDs, optionally excluding one.
    /// </summary>
    /// <param name="except">Optional player id to exclude.</param>
    /// <returns>Tracked player id sequence.</returns>
    public IEnumerable<uint> ExistingPlayersExcept(uint? except = null)
    {
        return except.HasValue ? _players.Where(id => id != except.Value) : _players;
    }

    /// <summary>
    /// Build the join notification packets for every player except the given
    /// ID.
    /// </summary>
    /// <param name="except">Player id that should not be included.</param>
    /// <returns>Join packets for all existing players except the provided id.</returns>
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
