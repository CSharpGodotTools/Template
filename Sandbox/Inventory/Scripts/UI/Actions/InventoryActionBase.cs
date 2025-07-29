using Godot;
using System;

namespace __TEMPLATE__.Inventory;

public abstract class InventoryActionBase
{
    protected InventoryContext Context { get; private set; }
    protected MouseButton MouseButton { get; private set; }
    protected int Index { get; private set; }
    
    private event Action<InventoryActionEventArgs> OnPreAction;
    private event Action<InventoryActionEventArgs> OnPostAction;

    public void Initialize(InventoryContext context, MouseButton mouseBtn, int index, Action<InventoryActionEventArgs> onPreAction, Action<InventoryActionEventArgs> onPostAction)
    {
        Context = context;
        MouseButton = mouseBtn;
        Index = index;
        OnPreAction = onPreAction;
        OnPostAction = onPostAction;
    }

    public abstract void Execute();

    protected void InvokeOnPreAction(InventoryActionEventArgs args)
    {
        OnPreAction?.Invoke(args);
    }

    protected void InvokeOnPostAction(InventoryActionEventArgs args)
    {
        OnPostAction?.Invoke(args);
    }
}
