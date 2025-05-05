namespace Template.Inventory;

public class InventoryAnimationPickup : InventoryAnimationBase
{
    public override void OnPreAnimate(InventoryActionEventArgs args)
    {
        itemFrame = args.TargetInventoryContainer.ItemContainers[args.FromIndex].GetCurrentSpriteFrame();

        vfxContainer = _context.VFX.AnimatePickup(args.TargetInventoryContainer, _context, args.FromIndex, itemFrame);
    }

    public override void OnPostAnimate(InventoryActionEventArgs args)
    {
        // Ensure the count is correctly displayed
        vfxContainer.SetCount(_context.CursorInventory.GetItem(0).Count);

        CursorItemContainer cursorItemContainer = _context.CursorItemContainer;

        cursorItemContainer.HideSpriteAndCount(); // Needed for visual effects to work
        cursorItemContainer.SetCurrentSpriteFrame(itemFrame);

        // Ensure cursorItemContainer's position is in the correct position
        cursorItemContainer.Position = args.TargetInventoryContainer.ItemContainers[args.FromIndex].GlobalPosition;
    }
}
