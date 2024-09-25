﻿using Godot;
using GodotUtils;
using System.Collections.Generic;

namespace Template.Inventory;

public class UIInventory
{
    private List<ItemContainer> _itemContainers = [];
    private CursorManager _cursorManager;

    public UIInventory(Inventory inventory, Node parent)
    {
        // Create the InventoryContainer
        InventoryContainer invContainer = InventoryContainer.Instantiate();

        // Add InventoryContainer to the tree as soon as possible so _Ready() functions in
        // children nodes will get called at the correct times
        parent.AddChild(invContainer);

        // Add the item containers
        AddItemContainers(invContainer, inventory);

        inventory.OnItemChanged += (index, item) =>
        {
            _itemContainers[index].SetItem(item);
        };

        _cursorManager = new CursorManager(parent.GetSceneNode<CursorItemContainer>());
    }

    private void AddItemContainers(InventoryContainer invContainer, Inventory inv)
    {
        for (int i = 0; i < inv.GetInventorySize(); i++)
        {
            AddItemContainer(new InventorySlotContext(inv, invContainer.AddItemContainer(), i));
        }
    }

    private void AddItemContainer(InventorySlotContext context)
    {
        ItemContainer itemContainer = context.ItemContainer;
        itemContainer.SetItem(context.Inventory.GetItem(context.Index));

        itemContainer.MouseEntered += () =>
        {

        };

        itemContainer.MouseExited += () =>
        {

        };

        itemContainer.GuiInput += inputEvent =>
        {
            if (inputEvent is InputEventMouseButton mouseBtn)
            {
                if (mouseBtn.IsLeftClickPressed())
                {
                    HandleLeftClick(context);
                }
            }
        };

        _itemContainers.Add(itemContainer);
    }

    private void HandleLeftClick(InventorySlotContext context)
    {
        Inventory inv = context.Inventory;
        int index = context.Index;

        if (_cursorManager.HasItem())
        {
            if (inv.HasItem(index))
            {
                
            }
            else
            {
                HandlePlaceItem(context);
            }
        }
        else
        {
            if (inv.HasItem(index))
            {
                HandlePickupItem(context);
            }
        }
    }

    private void HandlePlaceItem(InventorySlotContext context)
    {
        // Get the item and sprite frame before clearing the item from the cursor
        Item cursorItem = _cursorManager.GetItem();
        int spriteFrame = _cursorManager.GetCurrentSpriteFrame();

        // Clear the item from the cursor
        _cursorManager.ClearItem();

        // Set the inventory item
        context.Inventory.SetItem(context.Index, cursorItem);
        context.ItemContainer.SetCurrentSpriteFrame(spriteFrame);
    }

    private void HandlePickupItem(InventorySlotContext context)
    {
        Inventory inv = context.Inventory;
        
        // Get the item and sprite frame before clearing the item from the inventory
        Item item = inv.GetItem(context.Index);
        int spriteFrame = context.ItemContainer.GetCurrentSpriteFrame();

        // Clear the item from the inventory
        inv.ClearItem(context.Index);

        // Set the cursor item
        _cursorManager.SetItem(item, context.ItemContainer.GlobalPosition, spriteFrame);
    }
}
