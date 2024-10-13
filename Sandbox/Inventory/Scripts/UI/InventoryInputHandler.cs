﻿using Godot;
using GodotUtils;
using System;

namespace Template.Inventory;

public class InventoryInputHandler
{
    public event Action<InventoryActionEventArgs> OnPreInventoryAction, OnPostInventoryAction;

    private Action<MouseButton, InventoryAction, int> _onInput;
    private Action _hotbarInputs;
    
    private InventoryActionFactory _actionFactory;
    private InventoryContainer _invContainerPlayer;
    private InventoryContext _context;
    private Inventory _invPlayer;

    private ItemUnderCursor _itemUnderCursor;

    public InventoryInputHandler(int columns, InventoryContext context)
    {
        InventorySandbox sandbox = Services.Get<InventorySandbox>();

        _context = context;
        _actionFactory = new InventoryActionFactory();
        _invContainerPlayer = sandbox.GetPlayerInventory();
        _invPlayer = _invContainerPlayer.Inventory;

        for (int i = 0; i < columns; i++)
        {
            int index = i; // capture i

            _hotbarInputs += () =>
            {
                if (Input.IsActionJustPressed("hotbar_" + (index + 1)))
                {
                    int hotbarSlot = _invContainerPlayer.GetHotbarSlot(index);
                    _context.Inventory.MovePartOfItemTo(_invPlayer, _itemUnderCursor.Index, hotbarSlot, _itemUnderCursor.ItemStack.Count);
                }
            };
        }
    }

    public void Update()
    {
        if (_itemUnderCursor != null)
        {
            _hotbarInputs();
        }
    }

    public void RegisterInput(InventoryContainer container)
    {
        Inventory inventory = _context.Inventory;
        Inventory cursorInventory = _context.CursorInventory;

        _onInput += (mouseBtn, action, index) =>
        {
            InventoryActionEventArgs args = new(action);

            InventoryActionBase invAction = _actionFactory.GetAction(action);
            invAction.Initialize(_context, mouseBtn, index, OnPreInventoryAction, OnPostInventoryAction);
            invAction.Execute();
        };
    }

    public void HandleGuiInput(InventoryContainer container, InputEvent @event, int index)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.DoubleClick)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    _onInput?.Invoke(MouseButton.Left, InventoryAction.DoubleClick, index);
                }
                else if (mouseButton.ButtonIndex == MouseButton.Right)
                {
                    _onInput?.Invoke(MouseButton.Right, InventoryAction.DoubleClick, index);
                }
            }
            else if (mouseButton.IsLeftClickJustPressed())
            {
                HandleClick(container, new InputContext(_context.Inventory, _context.CursorInventory, MouseButton.Left, index));
            }
            else if (mouseButton.IsRightClickJustPressed())
            {
                HandleClick(container, new InputContext(_context.Inventory, _context.CursorInventory, MouseButton.Right, index));
            }
        }
    }

    public void HandleMouseEntered(InventoryContainer container, InventoryVFXManager vfxManager, int index, Vector2 mousePos)
    {
        _itemUnderCursor = new ItemUnderCursor(index, _context.Inventory.GetItem(index));

        if (_context.InputDetector.HoldingLeftClick)
        {
            if (_itemUnderCursor.ItemStack != null)
            {
                if (_context.InputDetector.HoldingShift)
                {
                    _onInput?.Invoke(MouseButton.Left, InventoryAction.Transfer, index);
                }
                else
                {
                    vfxManager.AnimateDragPickup(_context, index);
                    _context.CursorInventory.TakePartOfItemFrom(_context.Inventory, index, 0, _itemUnderCursor.ItemStack.Count);
                }
            }
        }
        else if (_context.InputDetector.HoldingRightClick)
        {
            vfxManager.AnimateDragPlace(_context, index, mousePos);
            _context.CursorInventory.MovePartOfItemTo(_context.Inventory, 0, index, 1);
        }
    }

    public void HandleMouseExited()
    {
        _itemUnderCursor = null;
    }

    private void HandleClick(InventoryContainer container, InputContext context)
    {
        if (context.CursorInventory.TryGetItem(0, out ItemStack cursorItem))
        {
            CursorHasItem(context, cursorItem);
        }
        else
        {
            CursorHasNoItem(container, context);
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

    private void CursorHasNoItem(InventoryContainer container, InputContext context)
    {
        if (context.Inventory.HasItem(context.Index))
        {
            if (_context.InputDetector.HoldingShift && context.MouseButton == MouseButton.Left)
            {
                TransferToOtherInventory(context.MouseButton, context.Index);
            }
            else
            {
                Pickup(container, context.MouseButton, context.Index);
            }
        }
    }

    private void TransferToOtherInventory(MouseButton mouseBtn, int index)
    {
        _onInput(mouseBtn, InventoryAction.Transfer, index);
    }

    private void Stack(MouseButton mouseBtn, int index)
    {
        //OnPreStack?.Invoke(index);
        _onInput(mouseBtn, InventoryAction.Stack, index);
        //OnPostStack?.Invoke(index);
    }

    private void Swap(MouseButton mouseBtn, int index)
    {
        // Swapping is disabled for right click operations
        if (mouseBtn == MouseButton.Right)
        {
            return;
        }

        //OnPreSwap?.Invoke(index);
        _onInput(mouseBtn, InventoryAction.Swap, index);
        //OnPostSwap?.Invoke(index);
    }

    private void Place(MouseButton mouseBtn, int index)
    {
        //OnPrePlace?.Invoke(index);
        _onInput(mouseBtn, InventoryAction.Place, index);
        //OnPostPlace?.Invoke(index);
    }

    private void Pickup(InventoryContainer container, MouseButton mouseBtn, int index)
    {
        //OnPrePickup?.Invoke(container, index);
        _onInput(mouseBtn, InventoryAction.Pickup, index);
        //OnPostPickup?.Invoke(container, index);
    }

    private class InputContext(Inventory inventory, Inventory cursorInventory, MouseButton mouseBtn, int index)
    {
        public Inventory Inventory { get; } = inventory;
        public Inventory CursorInventory { get; } = cursorInventory;
        public MouseButton MouseButton { get; } = mouseBtn;
        public int Index { get; } = index;
    }

    private class ItemUnderCursor(int index, ItemStack itemStack)
    {
        public int Index { get; } = index;
        public ItemStack ItemStack { get; } = itemStack;
    }
}
