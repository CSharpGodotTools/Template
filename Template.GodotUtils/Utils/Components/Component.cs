using Godot;
using System;
using System.Threading.Tasks;

namespace GodotUtils;

/// <summary>
/// Base class for lightweight components attached to a node.
/// </summary>
public class Component : IDisposable
{
    protected Node Owner;
    private ComponentManager _componentManager;
    private bool _disposed;

    /// <summary>
    /// Gets whether this component is blocked while the tree is paused.
    /// </summary>
    public bool IsPausable { get; private set; } = true;

    /// <summary>
    /// Creates a component attached to the provided owner node.
    /// </summary>
    public Component(Node owner)
    {
        Owner = owner;
        Owner.Ready += InitializeComponent;
        Owner.TreeExited += CleanupOnTreeExit;
    }

    /// <summary>
    /// Called when the owner node is ready.
    /// </summary>
    protected internal virtual void Ready() { }

    /// <summary>
    /// Called one frame after <see cref="Ready"/>.
    /// </summary>
    protected internal virtual void Deferred() { }

    /// <summary>
    /// Called every frame while processing is enabled.
    /// </summary>
    protected internal virtual void Process(double delta) { }

    /// <summary>
    /// Called every physics frame while physics processing is enabled.
    /// </summary>
    protected internal virtual void PhysicsProcess(double delta) { }

    /// <summary>
    /// Called when an input event is received while input is enabled.
    /// </summary>
    protected internal virtual void ProcessInput(InputEvent @event) { }

    /// <summary>
    /// Called when an unhandled input event is received while input is enabled.
    /// </summary>
    protected internal virtual void UnhandledInput(InputEvent @event) { }

    /// <summary>
    /// Called when the owner node exits the tree.
    /// </summary>
    protected internal virtual void OnDispose() { }

    /// <summary>
    /// Enables or disables all processing callbacks for this component.
    /// </summary>
    public void SetActive(bool active)
    {
        SetProcess(active);
        SetPhysicsProcess(active);
        SetInput(active);
        SetUnhandledInput(active);
    }

    /// <summary>
    /// Sets whether this component is blocked while the tree is paused.
    /// </summary>
    protected void SetPausable(bool enabled = true)
    {
        IsPausable = enabled;
        _componentManager?.OnPausableChanged(this);
    }

    /// <summary>
    /// Enables or disables per-frame processing.
    /// </summary>
    protected void SetProcess(bool enabled)
    {
        if (enabled)
            _componentManager.RegisterProcess(this);
        else
            _componentManager.UnregisterProcess(this);
    }

    /// <summary>
    /// Enables or disables physics processing.
    /// </summary>
    protected void SetPhysicsProcess(bool enabled)
    {
        if (enabled)
            _componentManager.RegisterPhysicsProcess(this);
        else
            _componentManager.UnregisterPhysicsProcess(this);
    }

    /// <summary>
    /// Enables or disables input processing.
    /// </summary>
    protected void SetInput(bool enabled)
    {
        if (enabled)
            _componentManager.RegisterInput(this);
        else
            _componentManager.UnregisterInput(this);
    }

    /// <summary>
    /// Enables or disables unhandled input processing.
    /// </summary>
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
        _componentManager = ComponentManager.Instance;
        Ready();
        TaskUtils.FireAndForget(() => CallNextFrame(Deferred));
    }

    private void CleanupOnTreeExit()
    {
        Dispose();
    }

    /// <summary>
    /// Performs component cleanup.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            OnDispose();
        }
        finally
        {
            _componentManager?.UnregisterAll(this);

            if (Owner != null)
            {
                Owner.Ready -= InitializeComponent;
                Owner.TreeExited -= CleanupOnTreeExit;
            }

            GC.SuppressFinalize(this);
        }
    }
}
