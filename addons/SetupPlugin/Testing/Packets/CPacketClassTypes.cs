using Framework.Netcode;
using System;
using System.Collections.Generic;

namespace Template.Setup.Testing;

public sealed class AbilityState
{
    public string AbilityId { get; set; }
    public int CooldownTicks { get; set; }
    public bool IsEnabled { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not AbilityState other)
        {
            return false;
        }

        return string.Equals(AbilityId, other.AbilityId, StringComparison.Ordinal) &&
            CooldownTicks == other.CooldownTicks &&
            IsEnabled == other.IsEnabled;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(AbilityId, StringComparer.Ordinal);
        hash.Add(CooldownTicks);
        hash.Add(IsEnabled);
        return hash.ToHashCode();
    }
}

public sealed class PlayerClassState
{
    public int PlayerId { get; set; }
    public string DisplayName { get; set; }
    public AbilityState PrimaryAbility { get; set; }
    public AbilityState SecondaryAbility { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not PlayerClassState other)
        {
            return false;
        }

        return PlayerId == other.PlayerId &&
            string.Equals(DisplayName, other.DisplayName, StringComparison.Ordinal) &&
            EqualityComparer<AbilityState>.Default.Equals(PrimaryAbility, other.PrimaryAbility) &&
            EqualityComparer<AbilityState>.Default.Equals(SecondaryAbility, other.SecondaryAbility);
    }

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

public partial class CPacketClassTypes : ClientPacket
{
    public PlayerClassState Current { get; set; }
    public PlayerClassState Previous { get; set; }
}
