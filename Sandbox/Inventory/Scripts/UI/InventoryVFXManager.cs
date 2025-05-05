using Godot;

namespace Template.Inventory;

public class InventoryVFXManager
{
    public void RegisterEvents(InventoryInputHandler input, InventoryContext context, InventoryContainer container)
    {
        InventoryAnimationFactory animationFactory = new(context, container);

        input.OnPreInventoryAction += args =>
        {
            animationFactory.GetAnimation(args.Action).OnPreAnimate(args);
        };

        input.OnPostInventoryAction += args =>
        {
            animationFactory.GetAnimation(args.Action).OnPostAnimate(args);
        };
    }

    public void AnimateDragPickup(InventoryContext context, int index)
    {
        Inventory cursorInventory = context.CursorInventory;
        Inventory inventory = context.Inventory;

        if (cursorInventory.HasItem(0) && !cursorInventory.GetItem(0).Material.Equals(inventory.GetItem(index).Material))
        {
            // Do nothing
        }
        else
        {
            context.VFX.AnimateDragPickup(context, index);
        }
    }

    public void AnimateDragPlace(InventoryContext context, int index, Vector2 mousePos)
    {
        Inventory cursorInventory = context.CursorInventory;
        Inventory inventory = context.Inventory;

        // Only do animations when the cursor has a item and the inventory does
        // not have an item. Otherwise too many animations gets too visually
        // chaotic.
        if (cursorInventory.HasItem(0) && !inventory.HasItem(index))
        {
            context.VFX.AnimateDragPlace(context, index, mousePos);
        }
    }
}
