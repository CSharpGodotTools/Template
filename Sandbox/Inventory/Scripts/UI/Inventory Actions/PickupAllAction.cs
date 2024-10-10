﻿using Godot;
using System;
using System.Collections.Generic;

namespace Template.Inventory;

public class PickupAllAction : InventoryActionBase
{
    public override void Execute()
    {
        if (MouseButton == MouseButton.Left)
        {
            InventoryContainer container = Context.InventoryContainer;
            Inventory cursorInventory = Context.CursorInventory;
            Inventory inventory = Context.Inventory;

            Material? material = cursorInventory.GetItem(0)?.Material ?? inventory.GetItem(Index)?.Material;

            if (material == null)
                return;

            InventoryContainer otherInvContainer = Services.Get<InventorySandbox>().GetOtherInventory(container);

            Dictionary<InventoryContainer, List<(int, ItemStack)>> items = [];
            items[container] = [];
            items[otherInvContainer] = [];

            foreach ((int i, ItemStack item) in inventory.GetItems())
            {
                if (item.Material.Equals(material))
                {
                    items[container].Add((i, item));
                }
            }

            foreach ((int i, ItemStack item) in otherInvContainer.Inventory.GetItems())
            {
                if (item.Material.Equals(material))
                {
                    items[otherInvContainer].Add((i, item));
                }
            }

            int sameItemCount = items[container].Count + items[otherInvContainer].Count;

            if (sameItemCount == 0)
                return;

            foreach ((int i, ItemStack item) in items[container])
            {
                // Do not animate index under cursor
                if (i == Index)
                    continue;

                InventoryActionEventArgs args = new(InventoryAction.Pickup);
                args.FromIndex = i;
                args.TargetInventoryContainer = container;

                InvokeOnPreAction(args);
                cursorInventory.TakeItemFrom(inventory, i, 0);
                InvokeOnPostAction(args);
            }

            foreach ((int i, ItemStack item) in items[otherInvContainer])
            {
                InventoryActionEventArgs args = new(InventoryAction.Pickup);
                args.FromIndex = i;
                args.TargetInventoryContainer = otherInvContainer;

                InvokeOnPreAction(args);
                cursorInventory.TakeItemFrom(otherInvContainer.Inventory, i, 0);
                InvokeOnPostAction(args);
            }
        }
    }
}
