using Godot;
using System;

namespace Template.Inventory;

public abstract class InventoryActionBase()
{
    protected InventoryContext _context { get; private set; }
    protected MouseButton _mouseButton { get; private set; }
    protected int _index { get; private set; }
    
    private event Action<InventoryActionEventArgs> _onPreAction;
    private event Action<InventoryActionEventArgs> _onPostAction;

    public void Initialize(InventoryContext context, MouseButton mouseBtn, int index, Action<InventoryActionEventArgs> onPreAction, Action<InventoryActionEventArgs> onPostAction)
    {
        _context = context;
        _mouseButton = mouseBtn;
        _index = index;
        _onPreAction = onPreAction;
        _onPostAction = onPostAction;
    }

    public abstract void Execute();

    protected void InvokeOnPreAction(InventoryActionEventArgs args)
    {
        _onPreAction?.Invoke(args);
    }

    protected void InvokeOnPostAction(InventoryActionEventArgs args)
    {
        _onPostAction?.Invoke(args);
    }
}
