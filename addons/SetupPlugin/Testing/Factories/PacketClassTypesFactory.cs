namespace Template.Setup.Testing;

public static class PacketClassTypesFactory
{
    public static CPacketClassTypes CreateSample()
    {
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
