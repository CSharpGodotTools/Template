using Godot;

namespace __TEMPLATE__.Inventory;

public class InventoryActionStack : InventoryActionBase
{
    public override void Execute()
    {
        InventoryActionEventArgs args = new(InventoryAction.Stack)
        {
            FromIndex = Index
        };

        InvokeOnPreAction(args);

        if (MouseButton == MouseButton.Left)
        {
            // Stack the entire cursor item stack
            Context.CursorInventory.MovePartOfItemTo(Context.Inventory, 0, Index, Context.CursorInventory.GetItem(0).Count);
        }
        else if (MouseButton == MouseButton.Right)
        {
            // Stack a single item
            Context.CursorInventory.MovePartOfItemTo(Context.Inventory, 0, Index, 1);
        }

        InvokeOnPostAction(args);
    }
}
