namespace __TEMPLATE__.Inventory;

public class InventoryAnimationTransfer : InventoryAnimationBase
{
    public override void OnPreAnimate(InventoryActionEventArgs args)
    {
        InventoryVFX.AnimateTransfer(_context, args.TargetInventoryContainer.ItemContainers[args.ToIndex], args.FromIndex);
    }

    public override void OnPostAnimate(InventoryActionEventArgs args)
    {
        if (!args.AreSameType)
        {
            args.TargetInventoryContainer.ItemContainers[args.ToIndex].HideSpriteAndCount();
        }
    }
}
