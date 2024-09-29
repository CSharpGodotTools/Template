﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Template.Inventory;

public class Inventory
{
    public event Action<int, ItemStack> OnItemChanged;
    
    private ItemStack[] _itemStacks;

    public Inventory(int itemSlotCount)
    {
        if (itemSlotCount <= 0)
        {
            throw new ArgumentException("Inventory item slot count must be greater than zero.");
        }

        _itemStacks = new ItemStack[itemSlotCount];
    }

    /// <summary>
    /// Remove all items from the inventory.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _itemStacks.Length; i++)
        {
            RemoveItem(i);
        }
    }

    /// <summary>
    /// Returns all non-empty <see cref="ItemStack"/>'s from this inventory.
    /// </summary>
    public IEnumerable<ItemStack> GetItems()
    {
        return _itemStacks.Where(item => item != null);
    }

    public void MoveItemTo(Inventory other, int fromIndex, int toIndex)
    {
        ItemTransfer(this, other, fromIndex, toIndex);
    }

    public void TakeItemFrom(Inventory other, int fromIndex, int toIndex)
    {
        ItemTransfer(other, this, fromIndex, toIndex);
    }

    public void SetItem(int index, ItemStack item)
    {
        ThrowIfIndexOutOfRange(index);

        _itemStacks[index] = item;

        NotifyItemChanged(index, item);
    }

    public void AddItem(ItemStack item)
    {
        // Try to stack the item with an existing item in the inventory
        if (TryStackItemFullSearch(item))
        {
            return;
        }

        // If the item cannot be stacked, try to place the item in the first empty slot
        if (TryFindFirstEmptySlot(out int index))
        {
            _itemStacks[index] = item;
            NotifyItemChanged(index, item);
        }
        else
        {
            GD.Print("Inventory is full.");
        }
    }


    public void RemoveItem(int index)
    {
        ThrowIfIndexOutOfRange(index);

        _itemStacks[index] = null;

        NotifyItemChanged(index, null);
    }

    public void SwapItems(int index1, int index2)
    {
        ThrowIfIndexOutOfRange(index1);
        ThrowIfIndexOutOfRange(index2);

        (_itemStacks[index2], _itemStacks[index1]) = (_itemStacks[index1], _itemStacks[index2]);

        NotifyItemChanged(index1, _itemStacks[index1]);
        NotifyItemChanged(index2, _itemStacks[index2]);
    }

    public bool HasItem(int index)
    {
        ThrowIfIndexOutOfRange(index);

        return _itemStacks[index] != null;
    }

    public ItemStack GetItem(int index)
    {
        ThrowIfIndexOutOfRange(index);

        return _itemStacks[index];
    }

    public int GetItemSlotCount()
    {
        return _itemStacks.Length;
    }

    public void DebugPrintInventory()
    {
        GD.Print(GetType().Name);

        for (int i = 0; i < _itemStacks.Length; i++)
        {
            GD.Print($"Slot {i}: {(_itemStacks[i] != null ? _itemStacks[i].ToString() : "Empty")}");
        }
    }

    public override string ToString()
    {
        return string.Join(' ', _itemStacks.Where(item => item != null));
    }

    private void NotifyItemChanged(int index, ItemStack item)
    {
        OnItemChanged?.Invoke(index, item);
    }

    private void ItemTransfer(Inventory source, Inventory destination, int fromIndex, int toIndex)
    {
        ItemStack sourceItem = source.GetItem(fromIndex);
        ItemStack destinationItem = destination.GetItem(toIndex);

        if (sourceItem != null && destinationItem != null)
        {
            if (sourceItem.Material.Equals(destinationItem.Material))
            {
                // Stack items
                destinationItem.Add(sourceItem.Count);
                destination.NotifyItemChanged(toIndex, destinationItem);

                source.RemoveItem(fromIndex);
                return;
            }
            else
            {
                // Swap items
                destination.SetItem(toIndex, sourceItem);
                source.SetItem(fromIndex, destinationItem);
                return;
            }
        }

        // Place or Pickup items
        destination.SetItem(toIndex, sourceItem);
        source.RemoveItem(fromIndex);
    }

    private void ThrowIfIndexOutOfRange(int index)
    {
        if (index < 0 || index >= _itemStacks.Length)
        {
            throw new IndexOutOfRangeException("Index out of range.");
        }
    }

    private bool TryStackItemFullSearch(ItemStack item)
    {
        for (int i = 0; i < _itemStacks.Length; i++)
        {
            if (_itemStacks[i] != null && _itemStacks[i].Material.Equals(item.Material))
            {
                _itemStacks[i].Add(item.Count);
                NotifyItemChanged(i, _itemStacks[i]);
                return true;
            }
        }

        return false;
    }

    private bool TryFindFirstEmptySlot(out int index)
    {
        for (int i = 0; i < _itemStacks.Length; i++)
        {
            if (_itemStacks[i] == null)
            {
                index = i;
                return true;
            }
        }

        index = -1;

        return false;
    }
}
