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
    public void RegisterProcess(Component component)
    {
        RegisterAndEnable(component, _process, _processPaused, _managerNode.SetProcess);
    }

    /// <summary>
    /// Unregisters a component from per-frame processing.
    /// </summary>
    public void UnregisterProcess(Component component)
    {
        UnregisterAndDisable(component, _process, _processPaused, _managerNode.SetProcess);
    }

    /// <summary>
    /// Registers a component for physics processing.
    /// </summary>
    public void RegisterPhysicsProcess(Component component)
    {
        RegisterAndEnable(component, _physicsProcess, _physicsProcessPaused, _managerNode.SetPhysicsProcess);
    }

    /// <summary>
    /// Unregisters a component from physics processing.
    /// </summary>
    public void UnregisterPhysicsProcess(Component component)
    {
        UnregisterAndDisable(component, _physicsProcess, _physicsProcessPaused, _managerNode.SetPhysicsProcess);
    }

    /// <summary>
    /// Registers a component for input processing.
    /// </summary>
    public void RegisterInput(Component component)
    {
        RegisterAndEnable(component, _input, _inputPaused, _managerNode.SetProcessInput);
    }

    /// <summary>
    /// Unregisters a component from input processing.
    /// </summary>
    public void UnregisterInput(Component component)
    {
        UnregisterAndDisable(component, _input, _inputPaused, _managerNode.SetProcessInput);
    }

    /// <summary>
    /// Registers a component for unhandled input processing.
    /// </summary>
    public void RegisterUnhandledInput(Component component)
    {
        RegisterAndEnable(component, _unhandledInput, _unhandledInputPaused, _managerNode.SetProcessUnhandledInput);
    }

    /// <summary>
    /// Unregisters a component from unhandled input processing.
    /// </summary>
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
    public void UnregisterAll(Component component)
    {
        UnregisterProcess(component);
        UnregisterPhysicsProcess(component);
        UnregisterInput(component);
        UnregisterUnhandledInput(component);
    }

    private static void Register(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup)
    {
        if (!mainGroup.Add(component))
            return;

        if (!component.IsPausable)
            pausedGroup.Add(component);
    }

    private static void Unregister(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup)
    {
        mainGroup.Remove(component);
        pausedGroup.Remove(component);
    }

    private static void SyncPausedGroup(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup)
    {
        if (!mainGroup.Contains(component))
            return;

        if (component.IsPausable)
            pausedGroup.Remove(component);
        else
            pausedGroup.Add(component);
    }

    private void Dispatch(ComponentGroup mainGroup, ComponentGroup pausedGroup, Action<Component> dispatch)
    {
        List<Component> items = _sceneTree.Paused ? pausedGroup.Items : mainGroup.Items;

        for (int i = items.Count - 1; i >= 0; i--)
            dispatch(items[i]);
    }

    private static void RegisterAndEnable(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup, Action<bool> setEnabled)
    {
        Register(component, mainGroup, pausedGroup);

        if (mainGroup.Count == 1)
            setEnabled(true);
    }

    private static void UnregisterAndDisable(Component component, ComponentGroup mainGroup, ComponentGroup pausedGroup, Action<bool> setEnabled)
    {
        Unregister(component, mainGroup, pausedGroup);

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
            if (!_lookup.Add(component))
                return false;

            _items.Add(component);
            return true;
        }

        public bool Remove(Component component)
        {
            if (!_lookup.Remove(component))
                return false;

            _items.Remove(component);
            return true;
        }
    }
}
