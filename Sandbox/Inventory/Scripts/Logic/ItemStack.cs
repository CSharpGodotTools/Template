using System;

namespace Template.Inventory;

public class ItemStack(Material material, int count)
{
    public Material Material { get; private set; } = material;
    public int Count { get; private set; } = count > 0 ? count : throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount to add must be greater than zero.");
        }

        Count += amount;
    }

    public bool Remove(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount to remove must be greater than zero.");
        }

        if (Count < amount)
        {
            return false; // Not enough items to remove
        }

        Count -= amount;
        return true;
    }

    public override string ToString()
    {
        return $"{Material} (x{Count})";
    }

    public bool Equals(ItemStack other)
    {
        if (other == null)
        {
            return false;
        }

        return Material.Equals(other.Material) && Count == other.Count;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        return Equals((ItemStack)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Material, Count);
    }
}
