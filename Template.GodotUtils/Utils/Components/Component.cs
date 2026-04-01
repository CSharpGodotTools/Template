using Godot;

namespace GodotUtils;

/// <summary>
/// Base class for lightweight components attached to a node.
/// </summary>
public class Component
{
    /// <summary>
    /// The node this component is attached to.
    /// </summary>
    protected Node Owner { get; }
    private ComponentManager _componentManager = null!;

    /// <summary>
    /// Gets whether this component is blocked while the tree is paused.
    /// </summary>
    public bool IsPausable { get; private set; } = true;

    /// <summary>
    /// Creates a component attached to the provided owner node.
    /// </summary>
    /// <param name="owner">Node that owns this component instance.</param>
    public Component(Node owner)
    {
        Owner = owner;
        Owner.Ready += OnReady;
        Owner.TreeExited += OnExitedTree;
    }

    /// <summary>
    /// Called when the owner node is ready.
    /// </summary>
    protected internal virtual void Ready() { }

    /// <summary>
    /// Called every frame while processing is enabled.
    /// </summary>
    /// <param name="delta">Frame delta time in seconds.</param>
    protected internal virtual void Process(double delta) { }

    /// <summary>
    /// Called every physics frame while physics processing is enabled.
    /// </summary>
    /// <param name="delta">Physics delta time in seconds.</param>
    protected internal virtual void PhysicsProcess(double delta) { }

    /// <summary>
    /// Called when an input event is received while input is enabled.
    /// </summary>
    /// <param name="event">Input event payload.</param>
    protected internal virtual void ProcessInput(InputEvent @event) { }

    /// <summary>
    /// Called when an unhandled input event is received while input is enabled.
    /// </summary>
    /// <param name="event">Input event payload.</param>
    protected internal virtual void UnhandledInput(InputEvent @event) { }

    /// <summary>
    /// Called when the owner node exits the tree.
    /// </summary>
    protected internal virtual void ExitTree() { }

    /// <summary>
    /// Enables or disables all processing callbacks for this component.
    /// </summary>
    /// <param name="active">True to enable callbacks; false to disable them.</param>
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
    /// <param name="enabled">True to pause this component with the tree.</param>
    protected void SetPausable(bool enabled = true)
    {
        IsPausable = enabled;
        _componentManager?.OnPausableChanged(this);
    }

    /// <summary>
    /// Enables or disables per-frame processing.
    /// </summary>
    /// <param name="enabled">True to register process callback.</param>
    protected void SetProcess(bool enabled)
    {
        // Register or unregister process callbacks based on requested state.
        if (enabled)
            _componentManager.RegisterProcess(this);
        else
            _componentManager.UnregisterProcess(this);
    }

    /// <summary>
    /// Enables or disables physics processing.
    /// </summary>
    /// <param name="enabled">True to register physics callback.</param>
    protected void SetPhysicsProcess(bool enabled)
    {
        // Register or unregister physics callbacks based on requested state.
        if (enabled)
            _componentManager.RegisterPhysicsProcess(this);
        else
            _componentManager.UnregisterPhysicsProcess(this);
    }

    /// <summary>
    /// Enables or disables input processing.
    /// </summary>
    /// <param name="enabled">True to register input callback.</param>
    protected void SetInput(bool enabled)
    {
        // Register or unregister input callbacks based on requested state.
        if (enabled)
            _componentManager.RegisterInput(this);
        else
            _componentManager.UnregisterInput(this);
    }

    /// <summary>
    /// Enables or disables unhandled input processing.
    /// </summary>
    /// <param name="enabled">True to register unhandled input callback.</param>
    protected void SetUnhandledInput(bool enabled)
    {
        // Register or unregister unhandled-input callbacks based on requested state.
        if (enabled)
            _componentManager.RegisterUnhandledInput(this);
        else
            _componentManager.UnregisterUnhandledInput(this);
    }

    /// <summary>
    /// Captures the active component manager and forwards to <see cref="Ready"/> when the owner node becomes ready.
    /// </summary>
    private void OnReady()
    {
        _componentManager = ComponentManager.Instance;
        Ready();
    }

    /// <summary>
    /// Forwards owner tree-exit notification and guarantees component cleanup and event unsubscription.
    /// </summary>
    private void OnExitedTree()
    {
        try
        {
            ExitTree();
        }
        finally
        {
            // Always unregister manager/state hooks even when ExitTree throws.
            _componentManager.UnregisterAll(this);
            Owner.Ready -= OnReady;
            Owner.TreeExited -= OnExitedTree;
        }
    }
}
