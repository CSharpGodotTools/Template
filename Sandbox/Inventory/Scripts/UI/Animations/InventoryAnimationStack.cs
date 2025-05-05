namespace Template.Inventory;

public class InventoryAnimationStack : InventoryAnimationBase
{
    public override void OnPreAnimate(InventoryActionEventArgs args)
    {
        itemFrame = _context.ItemContainers[args.FromIndex].GetCurrentSpriteFrame();
    }

    public override void OnPostAnimate(InventoryActionEventArgs args)
    {
        _context.ItemContainers[args.FromIndex].SetCurrentSpriteFrame(itemFrame);
    }
}
