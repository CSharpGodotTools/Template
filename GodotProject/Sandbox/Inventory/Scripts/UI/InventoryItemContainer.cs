﻿using Godot;
using GodotUtils;
using System;

namespace Template.Inventory;

public class InventoryItemContainer
{
    /// <summary>
    /// Occurs when the mouse cursor enters this container.
    /// </summary>
    public event Action<ItemContainerMouseEventArgs> MouseEntered;

    /// <summary>
    /// Occurs when the mouse cursor exits this container.
    /// </summary>
    public event Action<ItemContainerMouseEventArgs> MouseExited;

    public UIItem UIItem { get; private set; }
    public Control ItemParent { get; private set; }
    public InventoryContainer InventoryContainer { get; private set; }
    public Item Item { get; set; }

    public InventoryItemContainer(int index, float size, Node parent, InventoryContainer inventoryContainer)
    {
        InventoryContainer = inventoryContainer;

        PanelContainer container = new()
        {
            CustomMinimumSize = Vector2.One * size
        };

        container.MouseEntered += () =>
        {
            MouseEntered(new ItemContainerMouseEventArgs(index, this));
        };

        container.MouseExited += () =>
        {
            MouseExited(new ItemContainerMouseEventArgs(index, this));
        };

        ItemParent = AddCenterItemContainer(container);

        parent.AddChild(container);
    }

    public void SetItem(Item item)
    {
        ItemParent.QueueFreeChildren();

        ItemVisualData itemVisualData = ItemSpriteManager.GetResource(item);
        InventoryItemSprite sprite = ResourceFactoryRegistry.CreateSprite(itemVisualData, this);
        UIItem = sprite.UIItem;

        ItemParent.AddChild(sprite.Build());

        // Must be set after the sprite is added
        UIItem.SetItemCount(item.Count);
    }

    private Control AddCenterItemContainer(PanelContainer container)
    {
        CenterContainer center = new();
        Control control = new();

        container.AddChild(center);
        center.AddChild(control);

        return control;
    }
}
