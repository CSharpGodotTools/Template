using Godot;
using GodotUtils;
using System;
using System.Threading.Tasks;

namespace __TEMPLATE__;

public class Component : IDisposable
{
    protected Node Owner;
    private ComponentManager _componentManager;

    public Component(Node owner)
    {
        Owner = owner;
        Owner.Ready += InitializeComponent;
        Owner.TreeExited += CleanupOnTreeExit;
    }

    public virtual void Ready() { }
    public virtual void Deferred() { }
    public virtual void Process(double delta) { }
    public virtual void PhysicsProcess(double delta) { }
    public virtual void ProcessInput(InputEvent @event) { }
    public virtual void UnhandledInput(InputEvent @event) { }
    public virtual void Dispose() { }

    public void SetActive(bool active)
    {
        SetProcess(active);
        SetPhysicsProcess(active);
        SetInput(active);
        SetUnhandledInput(active);
    }

    protected void SetProcess(bool enabled)
    {
        if (enabled)
            _componentManager.RegisterProcess(this);
        else
            _componentManager.UnregisterProcess(this);
    }

    protected void SetPhysicsProcess(bool enabled)
    {
        if (enabled)
            _componentManager.RegisterPhysicsProcess(this);
        else
            _componentManager.UnregisterPhysicsProcess(this);
    }

    protected void SetInput(bool enabled)
    {
        if (enabled)
            _componentManager.RegisterInput(this);
        else
            _componentManager.UnregisterInput(this);
    }

    protected void SetUnhandledInput(bool enabled)
    {
        if (enabled)
            _componentManager.RegisterUnhandledInput(this);
        else
            _componentManager.UnregisterUnhandledInput(this);
    }

    private async Task CallNextFrame(Action action)
    {
        await Owner.WaitOneFrame();
        action();
    }

    private void InitializeComponent()
    {
        _componentManager = Owner.GetAutoload<Autoloads>(nameof(Autoloads)).ComponentManager;
        Ready();
        CallNextFrame(Deferred).FireAndForget();
    }

    private void CleanupOnTreeExit()
    {
        Dispose();
        _componentManager.UnregisterAll(this);
        Owner.Ready -= InitializeComponent;
        Owner.TreeExited -= CleanupOnTreeExit;
    }
}
