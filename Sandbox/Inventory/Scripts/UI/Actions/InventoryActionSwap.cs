using Godot;

namespace Template.Inventory;

public class InventoryActionSwap : InventoryActionBase
{
    public override void Execute()
    {
        InventoryActionEventArgs args = new(InventoryAction.Swap);
        args.FromIndex = _index;

        InvokeOnPreAction(args);

        if (_mouseButton == MouseButton.Left)
        {
            _context.CursorInventory.MoveItemTo(_context.Inventory, 0, _index);
        }
        else if (_mouseButton == MouseButton.Right)
        {
            // Right click swap disabled because inteferes with right click drag
        }

        InvokeOnPostAction(args);
    }
}
