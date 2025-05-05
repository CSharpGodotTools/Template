using Godot;

namespace Template.Inventory;

public class InventoryActionPlace : InventoryActionBase
{
    public override void Execute()
    {
        InventoryActionEventArgs args = new(InventoryAction.Place);
        args.FromIndex = _index;

        InvokeOnPreAction(args);

        if (_mouseButton == MouseButton.Left)
        {
            // Place the whole stack
            _context.CursorInventory.MoveItemTo(_context.Inventory, 0, _index);
        }
        else if (_mouseButton == MouseButton.Right)
        {
            // Place one item
            _context.CursorInventory.MovePartOfItemTo(_context.Inventory, 0, _index, 1);
        }

        InvokeOnPostAction(args);
    }
}
