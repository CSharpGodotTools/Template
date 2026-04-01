namespace Template.Setup.Testing;

/// <summary>
/// Builds class-based packet fixtures used by ENet round-trip tests.
/// </summary>
public static class PacketClassTypesFactory
{
    /// <summary>
    /// Creates a packet containing two fully populated class-state snapshots.
    /// </summary>
    /// <returns>Sample packet with current and previous player states.</returns>
    public static CPacketClassTypes CreateSample()
    {
        // Use distinct payloads so deep equality catches swapped references.
        PlayerClassState current = CreatePlayer(
            10,
            "Alpha",
            CreateAbility("dash", 3, true),
            CreateAbility("shield", 9, false));

        PlayerClassState previous = CreatePlayer(
            11,
            "Beta",
            CreateAbility("pulse", 5, true),
            CreateAbility("snare", 12, true));

        return new CPacketClassTypes
        {
            Current = current,
            Previous = previous
        };
    }

    /// <summary>
    /// Creates a sample player state with two configured abilities.
    /// </summary>
    /// <param name="playerId">Identifier assigned to the player.</param>
    /// <param name="displayName">Display name shown for the player.</param>
    /// <param name="primaryAbility">Primary ability snapshot.</param>
    /// <param name="secondaryAbility">Secondary ability snapshot.</param>
    /// <returns>Populated <see cref="PlayerClassState"/> instance.</returns>
    private static PlayerClassState CreatePlayer(
        int playerId,
        string displayName,
        AbilityState primaryAbility,
        AbilityState secondaryAbility)
    {
        return new PlayerClassState
        {
            PlayerId = playerId,
            DisplayName = displayName,
            PrimaryAbility = primaryAbility,
            SecondaryAbility = secondaryAbility
        };
    }

    /// <summary>
    /// Creates a sample ability state used inside class-state fixtures.
    /// </summary>
    /// <param name="abilityId">Stable ability identifier.</param>
    /// <param name="cooldownTicks">Remaining cooldown in ticks.</param>
    /// <param name="isEnabled">Whether the ability is currently enabled.</param>
    /// <returns>Populated <see cref="AbilityState"/> instance.</returns>
    private static AbilityState CreateAbility(string abilityId, int cooldownTicks, bool isEnabled)
    {
        return new AbilityState
        {
            AbilityId = abilityId,
            CooldownTicks = cooldownTicks,
            IsEnabled = isEnabled
        };
    }
}
