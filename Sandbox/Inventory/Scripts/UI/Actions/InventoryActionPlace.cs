using Godot;

namespace __TEMPLATE__.Inventory;

public class InventoryActionPlace : InventoryActionBase
{
    public override void Execute()
    {
        InventoryActionEventArgs args = new(InventoryAction.Place)
        {
            FromIndex = Index
        };

        InvokeOnPreAction(args);

        if (MouseButton == MouseButton.Left)
        {
            // Place the whole stack
            Context.CursorInventory.MoveItemTo(Context.Inventory, 0, Index);
        }
        else if (MouseButton == MouseButton.Right)
        {
            // Place one item
            Context.CursorInventory.MovePartOfItemTo(Context.Inventory, 0, Index, 1);
        }

        InvokeOnPostAction(args);
    }
}
