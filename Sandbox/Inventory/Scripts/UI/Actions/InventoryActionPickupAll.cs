using Godot;
using System.Collections.Generic;

namespace Template.Inventory;

public class InventoryActionPickupAll : InventoryActionBase
{
    public override void Execute()
    {
        if (_mouseButton == MouseButton.Left)
        {
            InventoryContainer container = _context.InventoryContainer;
            Inventory cursorInventory = _context.CursorInventory;
            Inventory inventory = _context.Inventory;

            Material? material = cursorInventory.GetItem(0)?.Material ?? inventory.GetItem(_index)?.Material;

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
                if (i == _index)
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
