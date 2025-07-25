﻿using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.Inventory;

public class InventoryInputHandler
{
    public event Action<InventoryActionEventArgs> OnPreInventoryAction, OnPostInventoryAction;

    private Action<MouseButton, InventoryAction, int> _onInput;
    private readonly Action _hotbarInputs;
    
    private readonly InventoryActionFactory _actionFactory;
    private readonly InventoryContext _context;

    private int _itemIndexUnderCursor;

    public InventoryInputHandler(int columns, InventoryContext context)
    {
        InventorySandbox sandbox = Services.Get<InventorySandbox>();

        _context = context;
        _actionFactory = new InventoryActionFactory();
        InventoryContainer invContainerPlayer = sandbox.GetPlayerInventory();
        Inventory invPlayer = invContainerPlayer.Inventory;

        for (int i = 0; i < columns; i++)
        {
            int index = i; // capture i

            _hotbarInputs += () =>
            {
                if (Input.IsActionJustPressed("hotbar_" + (index + 1)))
                {
                    // There is no item under the cursor
                    if (_itemIndexUnderCursor == -1)
                    {
                        return;
                    }

                    // Item under cursor is only updated on mouse enter exit events so it could
                    // be invalid if the cursor stays in the same slot after an inventory action
                    if (_context.Inventory.GetItem(_itemIndexUnderCursor) == null)
                    {
                        return;
                    }
                    
                    int hotbarSlotIndex = invContainerPlayer.GetHotbarSlot(index);

                    // I would like to replace the below line of code with
                    // _onInput?.Invoke(MouseButton.Left, InventoryAction.Swap, _itemIndexUnderCursor);
                    // but InventoryActionSwap.cs is specific to the cursor inventory meanwhile
                    // the below line of code is specific to the "world chest" and player inventories
                    _context.Inventory.MoveItemTo(invPlayer, _itemIndexUnderCursor, hotbarSlotIndex);
                }
            };
        }
    }

    public void Update()
    {
        _hotbarInputs();
    }

    public void RegisterInput()
    {
        _onInput += (mouseBtn, action, index) =>
        {
            InventoryActionBase invAction = InventoryActionFactory.GetAction(action);
            invAction.Initialize(_context, mouseBtn, index, OnPreInventoryAction, OnPostInventoryAction);
            invAction.Execute();
        };
    }

    public void HandleGuiInput(InputEvent @event, int index)
    {
        if (@event is not InputEventMouseButton mouseButton)
            return;

        if (mouseButton.DoubleClick)
        {
            switch (mouseButton.ButtonIndex)
            {
                case MouseButton.Left:
                    _onInput?.Invoke(MouseButton.Left, InventoryAction.DoubleClick, index);
                    break;
                case MouseButton.Right:
                    _onInput?.Invoke(MouseButton.Right, InventoryAction.DoubleClick, index);
                    break;
            }
        }
        else if (mouseButton.IsLeftClickJustPressed())
        {
            HandleClick(new InputContext(_context.Inventory, _context.CursorInventory, MouseButton.Left, index));
        }
        else if (mouseButton.IsRightClickJustPressed())
        {
            HandleClick(new InputContext(_context.Inventory, _context.CursorInventory, MouseButton.Right, index));
        }
    }

    public void HandleMouseEntered(int index, Vector2 mousePos)
    {
        _itemIndexUnderCursor = index;

        ItemStack itemUnderCursor = _context.Inventory.GetItem(index);

        if (_context.InputDetector.HoldingLeftClick)
        {
            if (itemUnderCursor != null)
            {
                if (_context.InputDetector.HoldingShift)
                {
                    _onInput?.Invoke(MouseButton.Left, InventoryAction.Transfer, index);
                }
                else
                {
                    InventoryVFXManager.AnimateDragPickup(_context, index);
                    _context.CursorInventory.TakePartOfItemFrom(_context.Inventory, index, 0, itemUnderCursor.Count);
                }
            }
        }
        else if (_context.InputDetector.HoldingRightClick)
        {
            InventoryVFXManager.AnimateDragPlace(_context, index, mousePos);
            _context.CursorInventory.MovePartOfItemTo(_context.Inventory, 0, index, 1);
        }
    }

    public void HandleMouseExited()
    {
        _itemIndexUnderCursor = -1;
    }

    private void HandleClick(InputContext context)
    {
        if (context.CursorInventory.TryGetItem(0, out ItemStack cursorItem))
        {
            CursorHasItem(context, cursorItem);
        }
        else
        {
            CursorHasNoItem(context);
        }
    }

    private void CursorHasItem(InputContext context, ItemStack cursorItem)
    {
        if (context.Inventory.TryGetItem(context.Index, out ItemStack invItem))
        {
            CursorAndInventoryHaveItems(context, invItem, cursorItem);
        }
        else
        {
            // Cursor has item but inv slot does not
            Place(context.MouseButton, context.Index);
        }
    }

    private void CursorAndInventoryHaveItems(InputContext context, ItemStack invItem, ItemStack cursorItem)
    {
        int index = context.Index;

        Material cursorMaterial = cursorItem.Material;
        Material invMaterial = invItem.Material;

        // The cursor item and inventory item are of the same type
        if (cursorMaterial.Equals(invMaterial))
        {
            Stack(context.MouseButton, index);
        }
        else
        {
            Swap(context.MouseButton, index);
        }
    }

    private void CursorHasNoItem(InputContext context)
    {
        if (context.Inventory.HasItem(context.Index))
        {
            if (_context.InputDetector.HoldingShift && context.MouseButton == MouseButton.Left)
            {
                TransferToOtherInventory(context.MouseButton, context.Index);
            }
            else
            {
                Pickup(context.MouseButton, context.Index);
            }
        }
    }

    private void TransferToOtherInventory(MouseButton mouseBtn, int index)
    {
        _onInput(mouseBtn, InventoryAction.Transfer, index);
    }

    private void Stack(MouseButton mouseBtn, int index)
    {
        _onInput(mouseBtn, InventoryAction.Stack, index);
    }

    private void Swap(MouseButton mouseBtn, int index)
    {
        // Swapping is disabled for right click operations
        if (mouseBtn == MouseButton.Right)
        {
            return;
        }

        _onInput(mouseBtn, InventoryAction.Swap, index);
    }

    private void Place(MouseButton mouseBtn, int index)
    {
        _onInput(mouseBtn, InventoryAction.Place, index);
    }

    private void Pickup(MouseButton mouseBtn, int index)
    {
        _onInput(mouseBtn, InventoryAction.Pickup, index);
    }

    private class InputContext(Inventory inventory, Inventory cursorInventory, MouseButton mouseBtn, int index)
    {
        public Inventory Inventory { get; } = inventory;
        public Inventory CursorInventory { get; } = cursorInventory;
        public MouseButton MouseButton { get; } = mouseBtn;
        public int Index { get; } = index;
    }
}
