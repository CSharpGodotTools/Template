﻿using Godot;

namespace Template.Inventory;

public class SwapAction : InventoryActionBase
{
    public override void Execute()
    {
        InventoryActionEventArgs args = new(InventoryAction.Swap);
        args.FromIndex = Index;

        InvokeOnPreAction(args);

        if (MouseButton == MouseButton.Left)
        {
            Context.CursorInventory.MoveItemTo(Context.Inventory, 0, Index);
        }
        else if (MouseButton == MouseButton.Right)
        {
            // Right click swap disabled because inteferes with right click drag
        }

        InvokeOnPostAction(args);
    }
}
