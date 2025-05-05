using Godot;

namespace Template.Inventory;

public class InventoryActionTransfer : InventoryActionBase
{
    public override void Execute()
    {
        if (_mouseButton == MouseButton.Left)
        {
            InventoryContainer otherInventoryContainer = Services.Get<InventorySandbox>().GetOtherInventory(_context.InventoryContainer);
            Inventory otherInventory = otherInventoryContainer.Inventory;

            if (otherInventory.TryFindFirstSameType(_context.Inventory.GetItem(_index).Material, out int stackIndex))
            {
                Transfer(true, otherInventoryContainer, otherInventory, stackIndex);
            }
            else if (otherInventory.TryFindFirstEmptySlot(out int otherIndex))
            {
                Transfer(false, otherInventoryContainer, otherInventory, otherIndex);
            }
        }
    }

    private void Transfer(bool areSameType, InventoryContainer otherInventoryContainer, Inventory otherInventory, int otherIndex)
    {
        ItemContainer targetItemContainer = otherInventoryContainer.ItemContainers[otherIndex];

        InventoryActionEventArgs args = new(InventoryAction.Transfer);
        args.TargetInventoryContainer = otherInventoryContainer;
        args.FromIndex = _index;
        args.ToIndex = otherIndex;
        args.AreSameType = areSameType;

        InvokeOnPreAction(args);

        _context.Inventory.MoveItemTo(otherInventory, _index, otherIndex);

        InvokeOnPostAction(args);
    }
}
