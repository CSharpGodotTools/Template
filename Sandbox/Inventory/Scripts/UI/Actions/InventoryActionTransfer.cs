using Godot;

namespace __TEMPLATE__.Inventory;

public class InventoryActionTransfer : InventoryActionBase
{
    public override void Execute()
    {
        if (MouseButton == MouseButton.Left)
        {
            InventoryContainer otherInventoryContainer = Services.Get<InventorySandbox>().GetOtherInventory(Context.InventoryContainer);
            Inventory otherInventory = otherInventoryContainer.Inventory;

            if (otherInventory.TryFindFirstSameType(Context.Inventory.GetItem(Index).Material, out int stackIndex))
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
        InventoryActionEventArgs args = new(InventoryAction.Transfer)
        {
            TargetInventoryContainer = otherInventoryContainer, 
            FromIndex = Index, 
            ToIndex = otherIndex, 
            AreSameType = areSameType
        };

        InvokeOnPreAction(args);

        Context.Inventory.MoveItemTo(otherInventory, Index, otherIndex);

        InvokeOnPostAction(args);
    }
}
