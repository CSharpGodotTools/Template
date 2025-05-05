using Godot;

namespace Template.Inventory;

public class InventoryActionStack : InventoryActionBase
{
    public override void Execute()
    {
        InventoryActionEventArgs args = new(InventoryAction.Stack);
        args.FromIndex = _index;

        InvokeOnPreAction(args);

        if (_mouseButton == MouseButton.Left)
        {
            // Stack the entire cursor item stack
            _context.CursorInventory.MovePartOfItemTo(_context.Inventory, 0, _index, _context.CursorInventory.GetItem(0).Count);
        }
        else if (_mouseButton == MouseButton.Right)
        {
            // Stack a single item
            _context.CursorInventory.MovePartOfItemTo(_context.Inventory, 0, _index, 1);
        }

        InvokeOnPostAction(args);
    }
}
