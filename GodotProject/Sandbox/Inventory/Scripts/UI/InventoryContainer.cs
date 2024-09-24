﻿using Godot;
using GodotUtils;

namespace Template.Inventory;

public class InventoryContainer
{
    public bool MouseIsOnSlot { get; private set; }
    public ItemContainerMouseEventArgs ActiveSlot { get; private set; }
    public Inventory Inventory { get; private set; }

    private InventoryItemContainer[] _itemContainers;

    public InventoryContainer(Inventory inventory, Node parent, int columns = 10)
    {
        Inventory = inventory;

        _itemContainers = new InventoryItemContainer[inventory.GetInventorySize()];

        PanelContainer container = new();
        GridContainer grid = AddGridContainer(container, columns);
        parent.AddChild(container);

        AddItems(inventory, grid);
    }

    public void SetItem(int index, Item item)
    {
        _itemContainers[index].SetItem(item);
    }

    private void AddItems(Inventory inventory, GridContainer grid)
    {
        const int ITEM_CONTAINER_PIXEL_SIZE = 50;

        for (int i = 0; i < inventory.GetInventorySize(); i++)
        {
            InventoryItemContainer container = new(i, ITEM_CONTAINER_PIXEL_SIZE, grid, this);
            _itemContainers[i] = container;

            container.MouseEntered += args =>
            {
                MouseIsOnSlot = true;
                ActiveSlot = args;
            };

            container.MouseExited += args =>
            {
                MouseIsOnSlot = false;
                ActiveSlot = args;
            };

            Item item = inventory.GetItem(i);

            if (item != null)
            {
                container.Item = item;
                SetItem(i, item);
            }
        }
    }

    private GridContainer AddGridContainer(PanelContainer container, int columns)
    {
        GMarginContainer margin = new();
        GridContainer grid = new();

        const int SEPARATION = 5;

        grid.Columns = columns;
        grid.AddThemeConstantOverride("h_separation", SEPARATION);
        grid.AddThemeConstantOverride("v_separation", SEPARATION);

        container.AddChild(margin);

        margin.AddChild(grid);
        margin.SetMarginAll(SEPARATION);

        return grid;
    }
}
