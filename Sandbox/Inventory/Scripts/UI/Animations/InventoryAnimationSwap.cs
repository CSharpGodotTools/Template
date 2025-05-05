namespace Template.Inventory;

public class InventoryAnimationSwap : InventoryAnimationBase
{
    public override void OnPreAnimate(InventoryActionEventArgs args)
    {
        itemFrame = _context.ItemContainers[args.FromIndex].GetCurrentSpriteFrame();
        cursorFrame = _context.CursorItemContainer.GetCurrentSpriteFrame();

        _context.VFX.AnimateSwap(_context, args.FromIndex, itemFrame, _container.GetGlobalMousePosition());
    }

    public override void OnPostAnimate(InventoryActionEventArgs args)
    {
        _context.ItemContainers[args.FromIndex].HideSpriteAndCount(); // Needed for visual effects to work
        _context.CursorItemContainer.HideSpriteAndCount(); // Needed for visual effects to work

        _context.ItemContainers[args.FromIndex].SetCurrentSpriteFrame(cursorFrame);
        _context.CursorItemContainer.SetCurrentSpriteFrame(itemFrame);

        // Ensure cursorItemContainer's position is in the correct position
        _context.CursorItemContainer.Position = _context.ItemContainers[args.FromIndex].GlobalPosition;
    }
}
