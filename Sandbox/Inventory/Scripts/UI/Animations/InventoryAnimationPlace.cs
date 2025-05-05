namespace Template.Inventory;

public class InventoryAnimationPlace : InventoryAnimationBase
{
    public override void OnPreAnimate(InventoryActionEventArgs args)
    {
        itemFrame = _context.CursorItemContainer.GetCurrentSpriteFrame();

        vfxContainer = _context.VFX.AnimatePlace(_context, args.FromIndex, itemFrame, _container.GetGlobalMousePosition());
    }

    public override void OnPostAnimate(InventoryActionEventArgs args)
    {
        // Ensure the count is correctly displayed
        vfxContainer.SetCount(_context.Inventory.GetItem(args.FromIndex).Count);

        _context.ItemContainers[args.FromIndex].HideSpriteAndCount(); // Needed for visual effects to work
        _context.ItemContainers[args.FromIndex].SetCurrentSpriteFrame(itemFrame);
    }
}
