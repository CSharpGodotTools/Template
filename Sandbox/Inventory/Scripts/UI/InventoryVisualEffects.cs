﻿using Godot;
using System.Collections.Generic;

namespace Template.Inventory;

public class InventoryVisualEffects()
{
    private List<Node> _swapAnimContainers = [];

    public void AnimateDragPickup(Context context, int index)
    {
        AnimHelperItemContainer container = new AnimHelperItemContainer.Builder(AnimHelperItemContainer.Instantiate())
            .SetInitialPositionForControl(context.ItemContainers[index].GlobalPosition)
            .SetTargetAsMouse()
            .SetStartingLerp(0.3f) // Need to make animation quick
            .SetItemAndFrame(context.Inventory.GetItem(index), 0)
            .SetCount(0) // Too much information on screen gets chaotic
            .Build();

        context.UI.AddChild(container);

        if (!context.CursorInventory.HasItem(0))
        {
            context.CursorItemContainer.HideSpriteAndCount();
        }

        container.OnReachedTarget += context.CursorItemContainer.ShowSpriteAndCount;
    }

    public void AnimateDragPlace(Context context, int index, Vector2 mousePos)
    {
        ItemContainer[] itemContainers = context.ItemContainers;

        // Place one of item from cursor to inventory slot
        AnimHelperItemContainer container = new AnimHelperItemContainer.Builder(AnimHelperItemContainer.Instantiate())
            .SetInitialPositionForNode2D(mousePos)
            .SetControlTarget(itemContainers[index].GlobalPosition)
            .SetItemAndFrame(context.CursorInventory.GetItem(0), 0)
            .SetCount(0) // Too much information on screen gets chaotic
            .Build();

        itemContainers[index].HideSpriteAndCount();

        container.OnReachedTarget += () =>
        {
            itemContainers[index].ShowSpriteAndCount();
        };

        context.UI.AddChild(container);
    }

    public AnimHelperItemContainer AnimatePickup(Context context, int index, int itemFrame)
    {
        AnimHelperItemContainer container = new AnimHelperItemContainer.Builder(AnimHelperItemContainer.Instantiate())
            .SetInitialPositionForControl(context.ItemContainers[index].GlobalPosition)
            .SetTargetAsMouse()
            .SetItemAndFrame(context.Inventory.GetItem(index), itemFrame)
            .Build();

        container.OnReachedTarget += context.CursorItemContainer.ShowSpriteAndCount;

        context.UI.AddChild(container);

        return container;
    }

    public AnimHelperItemContainer AnimatePlace(Context context, int index, int itemFrame, Vector2 mousePos)
    {
        AnimHelperItemContainer container = new AnimHelperItemContainer.Builder(AnimHelperItemContainer.Instantiate())
            .SetInitialPositionForNode2D(mousePos)
            .SetControlTarget(context.ItemContainers[index].GlobalPosition)
            .SetItemAndFrame(context.CursorInventory.GetItem(0), itemFrame)
            .Build();

        container.OnReachedTarget += () =>
        {
            context.ItemContainers[index].ShowSpriteAndCount();
        };

        context.UI.AddChild(container);

        return container;
    }

    public void AnimateSwap(Context context, int index, int itemFrame, Vector2 mousePos)
    {
        foreach (Node node in _swapAnimContainers)
        {
            if (GodotObject.IsInstanceValid(node))
            {
                node.QueueFree();
            }
        }

        _swapAnimContainers.Clear();

        ItemContainer[] itemContainers = context.ItemContainers;

        AnimHelperItemContainer container = new AnimHelperItemContainer.Builder(AnimHelperItemContainer.Instantiate())
            .SetInitialPositionForControl(itemContainers[index].GlobalPosition)
            .SetTargetAsMouse()
            .SetItemAndFrame(context.Inventory.GetItem(index), itemFrame)
            .Build();

        container.OnReachedTarget += context.CursorItemContainer.ShowSpriteAndCount;

        context.UI.AddChild(container);

        AnimHelperItemContainer container2 = new AnimHelperItemContainer.Builder(AnimHelperItemContainer.Instantiate())
            .SetInitialPositionForNode2D(mousePos)
            .SetControlTarget(itemContainers[index].GlobalPosition)
            .SetItemAndFrame(context.CursorInventory.GetItem(0), itemFrame)
            .Build();

        container2.OnReachedTarget += () =>
        {
            itemContainers[index].ShowSpriteAndCount();
        };

        context.UI.AddChild(container2);

        _swapAnimContainers.Add(container);
        _swapAnimContainers.Add(container2);
    }

    public class Context(CanvasLayer ui, ItemContainer[] itemContainers, CursorItemContainer cursorItemContainer, Inventory inventory, Inventory cursorInventory)
    {
        public CanvasLayer UI { get; } = ui;
        public ItemContainer[] ItemContainers { get; } = itemContainers;
        public CursorItemContainer CursorItemContainer { get; } = cursorItemContainer;
        public Inventory Inventory { get; } = inventory;
        public Inventory CursorInventory { get; } = cursorInventory;
    }
}
