using __TEMPLATE__.Netcode;
using System;
using System.Collections.Generic;

namespace Template.Setup.Testing;

/// <summary>
/// Represents an ability state snapshot used by class-type packet fixtures.
/// </summary>
public sealed class AbilityState
{
    /// <summary>
    /// Gets or sets the stable identifier for the ability.
    /// </summary>
    public string AbilityId { get; set; } = null!;

    /// <summary>
    /// Gets or sets remaining cooldown in simulation ticks.
    /// </summary>
    public int CooldownTicks { get; set; }

    /// <summary>
    /// Gets or sets whether the ability can currently be used.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Determines value equality with another <see cref="AbilityState"/>.
    /// </summary>
    /// <param name="obj">Object to compare against.</param>
    /// <returns><see langword="true"/> when all ability fields match.</returns>
    public override bool Equals(object? obj)
    {
        // Equality requires the other object to be an AbilityState instance.
        if (obj is not AbilityState other)
        {
            return false;
        }

        return string.Equals(AbilityId, other.AbilityId, StringComparison.Ordinal) &&
            CooldownTicks == other.CooldownTicks &&
            IsEnabled == other.IsEnabled;
    }

    /// <summary>
    /// Computes a hash code from all ability state fields.
    /// </summary>
    /// <returns>Combined hash code for this instance.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(AbilityId, StringComparer.Ordinal);
        hash.Add(CooldownTicks);
        hash.Add(IsEnabled);
        return hash.ToHashCode();
    }
}

/// <summary>
/// Represents class-based player state used by test packet payloads.
/// </summary>
public sealed class PlayerClassState
{
    /// <summary>
    /// Gets or sets the player identifier.
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the player display name.
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the primary ability state.
    /// </summary>
    public AbilityState PrimaryAbility { get; set; } = null!;

    /// <summary>
    /// Gets or sets the secondary ability state.
    /// </summary>
    public AbilityState SecondaryAbility { get; set; } = null!;

    /// <summary>
    /// Determines value equality with another <see cref="PlayerClassState"/>.
    /// </summary>
    /// <param name="obj">Object to compare against.</param>
    /// <returns><see langword="true"/> when all fields and nested values match.</returns>
    public override bool Equals(object? obj)
    {
        // Equality requires the other object to be a PlayerClassState instance.
        if (obj is not PlayerClassState other)
        {
            return false;
        }

        return PlayerId == other.PlayerId &&
            string.Equals(DisplayName, other.DisplayName, StringComparison.Ordinal) &&
            EqualityComparer<AbilityState>.Default.Equals(PrimaryAbility, other.PrimaryAbility) &&
            EqualityComparer<AbilityState>.Default.Equals(SecondaryAbility, other.SecondaryAbility);
    }

    /// <summary>
    /// Computes a hash code from player identity and nested ability state.
    /// </summary>
    /// <returns>Combined hash code for this instance.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(DisplayName, StringComparer.Ordinal);
        hash.Add(PrimaryAbility);
        hash.Add(SecondaryAbility);
        return hash.ToHashCode();
    }
}

/// <summary>
/// Packet carrying class-based player snapshots for round-trip tests.
/// </summary>
public partial class CPacketClassTypes : ClientPacket
{
    /// <summary>
    /// Gets or sets current player class state snapshot.
    /// </summary>
    public PlayerClassState Current { get; set; }

    /// <summary>
    /// Gets or sets previous player class state snapshot.
    /// </summary>
    public PlayerClassState Previous { get; set; }
}
