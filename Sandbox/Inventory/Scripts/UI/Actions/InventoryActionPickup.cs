using Godot;

namespace __TEMPLATE__.Inventory;

public class InventoryActionPickup : InventoryActionBase
{
    public override void Execute()
    {
        InventoryActionEventArgs args = new(InventoryAction.Pickup)
        {
            FromIndex = Index,
            TargetInventoryContainer = Context.InventoryContainer
        };

        InvokeOnPreAction(args);

        if (MouseButton == MouseButton.Left)
        {
            // Left click pickup logic
            Context.CursorInventory.TakeItemFrom(Context.Inventory, Index, 0);
        }
        else if (MouseButton == MouseButton.Right)
        {
            // Right click pickup (half stack or one item)
            Inventory cursorInventory = Context.CursorInventory;
            Inventory inventory = Context.Inventory;

            int halfItemCount = inventory.GetItem(Index).Count / 2;

            if (Context.InputDetector.HoldingShift && halfItemCount != 0)
            {
                cursorInventory.TakePartOfItemFrom(inventory, Index, 0, halfItemCount);
            }
            else
            {
                cursorInventory.TakePartOfItemFrom(inventory, Index, 0, 1);
            }
        }

        InvokeOnPostAction(args);
    }
}
