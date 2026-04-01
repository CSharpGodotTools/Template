using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Centralized component dispatch to reduce per-node callbacks.
/// </summary>
public class ComponentManager
{
    /// <summary>
    /// Gets the active component manager instance.
    /// </summary>
    public static ComponentManager Instance { get; private set; } = null!;

    private readonly ComponentGroup _process = new();
    private readonly ComponentGroup _processPaused = new();

    private readonly ComponentGroup _physicsProcess = new();
    private readonly ComponentGroup _physicsProcessPaused = new();

    private readonly ComponentGroup _input = new();
    private readonly ComponentGroup _inputPaused = new();

    private readonly ComponentGroup _unhandledInput = new();
    private readonly ComponentGroup _unhandledInputPaused = new();

    private readonly Node _managerNode;
    private readonly SceneTree _sceneTree;

    /// <summary>
    /// Creates a manager tied to the provided node.
    /// </summary>
    /// <param name="managerNode">Node that owns processing callbacks for dispatch.</param>
    public ComponentManager(Node managerNode)
    {
        _managerNode = managerNode;
        _sceneTree = managerNode.GetTree();
    }

    public void EnterTree()
    {
        _managerNode.SetProcess(false);
        _managerNode.SetPhysicsProcess(false);
        _managerNode.SetProcessInput(false);
        _managerNode.SetProcessUnhandledInput(false);
    }

    public void Ready()
    {
        Instance = this;
    }

    public void Process(double delta)
    {
        Dispatch(_process, _processPaused, component => component.Process(delta));
    }

    public void PhysicsProcess(double delta)
    {
        Dispatch(_physicsProcess, _physicsProcessPaused, component => component.PhysicsProcess(delta));
    }

    public void Input(InputEvent @event)
    {
        Dispatch(_input, _inputPaused, component => component.ProcessInput(@event));
    }

    public void UnhandledInput(InputEvent @event)
    {
        Dispatch(_unhandledInput, _unhandledInputPaused, component => component.UnhandledInput(@event));
    }

    /// <summary>
    /// Registers a component for per-frame processing.
    /// </summary>
    /// <param name="component">Component to register.</param>
    public void RegisterProcess(Component component)
    {
        RegisterAndEnable(component, _process, _processPaused, _managerNode.SetProcess);
    }

    /// <summary>
    /// Unregisters a component from per-frame processing.
    /// </summary>
    /// <param name="component">Component to unregister.</param>
    public void UnregisterProcess(Component component)
    {
        UnregisterAndDisable(component, _process, _processPaused, _managerNode.SetProcess);
    }

    /// <summary>
    /// Registers a component for physics processing.
    /// </summary>
    /// <param name="component">Component to register.</param>
    public void RegisterPhysicsProcess(Component component)
    {
        RegisterAndEnable(component, _physicsProcess, _physicsProcessPaused, _managerNode.SetPhysicsProcess);
    }

    /// <summary>
    /// Unregisters a component from physics processing.
    /// </summary>
    /// <param name="component">Component to unregister.</param>
    public void UnregisterPhysicsProcess(Component component)
    {
        UnregisterAndDisable(component, _physicsProcess, _physicsProcessPaused, _managerNode.SetPhysicsProcess);
    }

    /// <summary>
    /// Registers a component for input processing.
    /// </summary>
    /// <param name="component">Component to register.</param>
    public void RegisterInput(Component component)
    {
        RegisterAndEnable(component, _input, _inputPaused, _managerNode.SetProcessInput);
    }

    /// <summary>
    /// Unregisters a component from input processing.
    /// </summary>
    /// <param name="component">Component to unregister.</param>
    public void UnregisterInput(Component component)
    {
        UnregisterAndDisable(component, _input, _inputPaused, _managerNode.SetProcessInput);
    }

    /// <summary>
    /// Registers a component for unhandled input processing.
    /// </summary>
    /// <param name="component">Component to register.</param>
    public void RegisterUnhandledInput(Component component)
    {
        RegisterAndEnable(component, _unhandledInput, _unhandledInputPaused, _managerNode.SetProcessUnhandledInput);
    }

    /// <summary>
    /// Unregisters a component from unhandled input processing.
    /// </summary>
    /// <param name="component">Component to unregister.</param>
    public void UnregisterUnhandledInput(Component component)
    {
        UnregisterAndDisable(component, _unhandledInput, _unhandledInputPaused, _managerNode.SetProcessUnhandledInput);
    }

    internal void OnPausableChanged(Component component)
    {
        SyncPausedGroup(component, _process, _processPaused);
        SyncPausedGroup(component, _physicsProcess, _physicsProcessPaused);
        SyncPausedGroup(component, _input, _inputPaused);
        SyncPausedGroup(component, _unhandledInput, _unhandledInputPaused);
    }

    /// <summary>
    /// Unregisters a component from all processing types.
    /// </summary>
    /// <param name="component">Component to unregister from every dispatch group.</param>
    public void UnregisterAll(Component component)
    {
        UnregisterProcess(component);
        UnregisterPhysicsProcess(component);
        UnregisterInput(component);
        UnregisterUnhandledInput(component);
    }

    /// <summary>
    /// Adds a component to the main group and to the paused group when it should keep running while paused.
    /// </summary>
    /// <param name="component">Component to register.</param>
    /// <param name="mainGroup">Primary dispatch group.</param>
    /// <param name="pausedGroup">Dispatch group used while the tree is paused.</param>
    private static void Register(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup)
    {
        // Skip duplicate registrations of the same component.
        if (!mainGroup.Add(component))
            return;

        // Non-pausable components keep updating while the scene tree is paused.
        if (!component.IsPausable)
            pausedGroup.Add(component);
    }

    /// <summary>
    /// Removes a component from both the main and paused dispatch groups.
    /// </summary>
    /// <param name="component">Component to unregister.</param>
    /// <param name="mainGroup">Primary dispatch group.</param>
    /// <param name="pausedGroup">Dispatch group used while the tree is paused.</param>
    private static void Unregister(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup)
    {
        mainGroup.Remove(component);
        pausedGroup.Remove(component);
    }

    /// <summary>
    /// Keeps paused-group membership in sync with the component's pausable state.
    /// </summary>
    /// <param name="component">Component whose pausable state changed.</param>
    /// <param name="mainGroup">Primary dispatch group.</param>
    /// <param name="pausedGroup">Dispatch group used while the tree is paused.</param>
    private static void SyncPausedGroup(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup)
    {
        // Ignore components not registered for this dispatch group.
        if (!mainGroup.Contains(component))
            return;

        // Move component between paused and active lists based on pausable flag.
        if (component.IsPausable)
            pausedGroup.Remove(component);
        else
            pausedGroup.Add(component);
    }

    /// <summary>
    /// Dispatches a callback to either the main or paused group depending on scene-tree pause state.
    /// </summary>
    /// <param name="mainGroup">Primary dispatch group.</param>
    /// <param name="pausedGroup">Dispatch group used while the tree is paused.</param>
    /// <param name="dispatch">Callback invoked for each selected component.</param>
    private void Dispatch(ComponentGroup mainGroup, ComponentGroup pausedGroup, Action<Component> dispatch)
    {
        List<Component> items = _sceneTree.Paused ? pausedGroup.Items : mainGroup.Items;

        for (int i = items.Count - 1; i >= 0; i--)
            dispatch(items[i]);
    }

    /// <summary>
    /// Registers a component and enables the owning node callback when the first item is added.
    /// </summary>
    /// <param name="component">Component to register.</param>
    /// <param name="mainGroup">Primary dispatch group.</param>
    /// <param name="pausedGroup">Dispatch group used while the tree is paused.</param>
    /// <param name="setEnabled">Callback that toggles the corresponding node processing flag.</param>
    private static void RegisterAndEnable(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup, Action<bool> setEnabled)
    {
        Register(component, mainGroup, pausedGroup);

        // Enable manager callback only when first component is registered.
        if (mainGroup.Count == 1)
            setEnabled(true);
    }

    /// <summary>
    /// Unregisters a component and disables the owning node callback when no items remain.
    /// </summary>
    /// <param name="component">Component to unregister.</param>
    /// <param name="mainGroup">Primary dispatch group.</param>
    /// <param name="pausedGroup">Dispatch group used while the tree is paused.</param>
    /// <param name="setEnabled">Callback that toggles the corresponding node processing flag.</param>
    private static void UnregisterAndDisable(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup, Action<bool> setEnabled)
    {
        Unregister(component, mainGroup, pausedGroup);

        // Disable manager callback once no components remain.
        if (mainGroup.Count == 0)
            setEnabled(false);
    }

    private sealed class ComponentGroup
    {
        private readonly List<Component> _items = [];
        private readonly HashSet<Component> _lookup = [];

        public List<Component> Items => _items;
        public int Count => _items.Count;

        public bool Contains(Component component) => _lookup.Contains(component);

        public bool Add(Component component)
        {
            // Reject duplicates while preserving insertion order list.
            if (!_lookup.Add(component))
                return false;

            _items.Add(component);
            return true;
        }

        public bool Remove(Component component)
        {
            // Ignore remove requests for unknown components.
            if (!_lookup.Remove(component))
                return false;

            _items.Remove(component);
            return true;
        }
    }
}
