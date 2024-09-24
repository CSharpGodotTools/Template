﻿using Godot;
using GodotUtils;

namespace Template.Inventory;

[Draggable]
public partial class UIItem : AnimatedSprite2D, IDraggable
{
    public int Count { get; set; }

    private InventoryItemContainer _inventoryItemContainer;
    private Label _itemCountLabel;

    public override void _Ready()
    {
        _itemCountLabel = CreateItemCountLabel();
        AddChild(_itemCountLabel);
    }

    public void SetInventoryItemContainer(InventoryItemContainer container)
    {
        _inventoryItemContainer = container;
    }

    public void SetItemCount(int count)
    {
        Count = count;
        _itemCountLabel.Text = Count.ToString();
    }

    public void OnDragReleased()
    {
        if (_inventoryItemContainer.InventoryContainer.MouseIsOnSlot)
        {
            ItemContainerMouseEventArgs otherSlot = _inventoryItemContainer.InventoryContainer.ActiveSlot;
            InventoryItemContainer otherContainer = otherSlot.InventoryItemContainer;

            Inventory thisInventory = _inventoryItemContainer.InventoryContainer.Inventory;
            Inventory otherInventory = otherContainer.InventoryContainer.Inventory;

            Item thisItem = thisInventory.GetItem(_inventoryItemContainer.Index);
            Item otherItem = otherInventory.GetItem(otherContainer.Index);

            if (otherItem != null && thisItem != null && otherItem.Equals(thisItem))
            {
                // Combine counts if items are of the same type
                otherItem.Count += thisItem.Count;
                otherInventory.SetItem(otherContainer.Index, otherItem); // Update the inventory
                otherContainer.SetItem(otherItem); // Update the UI
                thisInventory.SetItem(_inventoryItemContainer.Index, null); // Clear the current item in the inventory
                _inventoryItemContainer.Item = null; // Clear the current item
                _inventoryItemContainer.ClearItemParent(); // Update the UI to reflect the absence of an item
            }
            else
            {
                // Swap items if they are not of the same type
                _inventoryItemContainer.SwapItems(otherContainer);
            }

            QueueFree();
        }
    }

    private Label CreateItemCountLabel()
    {
        Vector2 size = this.GetSize();

        Label label = new()
        {
            Text = "0",
            Scale = Vector2.One * 0.25f,
            Position = size * 0.1f + new Vector2(size.X * 0.3f, 0),
            LabelSettings = new LabelSettings
            {
                FontSize = 32,
            },
        };

        return label;
    }
}
