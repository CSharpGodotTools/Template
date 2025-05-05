using Godot;

namespace Template.Inventory;

public class InventoryActionPickup : InventoryActionBase
{
    public override void Execute()
    {
        InventoryActionEventArgs args = new(InventoryAction.Pickup);
        args.FromIndex = _index;
        args.TargetInventoryContainer = _context.InventoryContainer;

        InvokeOnPreAction(args);

        if (_mouseButton == MouseButton.Left)
        {
            // Left click pickup logic
            _context.CursorInventory.TakeItemFrom(_context.Inventory, _index, 0);
        }
        else if (_mouseButton == MouseButton.Right)
        {
            // Right click pickup (half stack or one item)
            Inventory cursorInventory = _context.CursorInventory;
            Inventory inventory = _context.Inventory;

            int halfItemCount = inventory.GetItem(_index).Count / 2;

            if (_context.InputDetector.HoldingShift && halfItemCount != 0)
            {
                cursorInventory.TakePartOfItemFrom(inventory, _index, 0, halfItemCount);
            }
            else
            {
                cursorInventory.TakePartOfItemFrom(inventory, _index, 0, 1);
            }
        }

        InvokeOnPostAction(args);
    }
}
