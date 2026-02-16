using Godot;
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
    public static ComponentManager Instance { get; private set; }

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
        _managerNode.SetProcessInput(false);
        _managerNode.SetProcessUnhandledInput(false);
    }

    public void Ready()
    {
        Instance = this;
    }

    public void Process(double delta)
    {
        List<Component> processList = _sceneTree.Paused ? _processPaused.Items : _process.Items;

        for (int i = processList.Count - 1; i >= 0; i--)
            processList[i].Process(delta);
    }

    public void PhysicsProcess(double delta)
    {
        List<Component> physicsProcessList = _sceneTree.Paused ? _physicsProcessPaused.Items : _physicsProcess.Items;

        for (int i = physicsProcessList.Count - 1; i >= 0; i--)
            physicsProcessList[i].PhysicsProcess(delta);
    }

    public void Input(InputEvent @event)
    {
        List<Component> inputList = _sceneTree.Paused ? _inputPaused.Items : _input.Items;

        for (int i = inputList.Count - 1; i >= 0; i--)
            inputList[i].ProcessInput(@event);
    }

    public void UnhandledInput(InputEvent @event)
    {
        List<Component> unhandledInputList = _sceneTree.Paused ? _unhandledInputPaused.Items : _unhandledInput.Items;

        for (int i = unhandledInputList.Count - 1; i >= 0; i--)
            unhandledInputList[i].UnhandledInput(@event);
    }

    /// <summary>
    /// Registers a component for per-frame processing.
    /// </summary>
    public void RegisterProcess(Component component)
    {
        Register(component, _process, _processPaused);
    }

    /// <summary>
    /// Unregisters a component from per-frame processing.
    /// </summary>
    public void UnregisterProcess(Component component)
    {
        Unregister(component, _process, _processPaused);
    }

    /// <summary>
    /// Registers a component for physics processing.
    /// </summary>
    public void RegisterPhysicsProcess(Component component)
    {
        Register(component, _physicsProcess, _physicsProcessPaused);
    }

    /// <summary>
    /// Unregisters a component from physics processing.
    /// </summary>
    public void UnregisterPhysicsProcess(Component component)
    {
        Unregister(component, _physicsProcess, _physicsProcessPaused);
    }

    /// <summary>
    /// Registers a component for input processing.
    /// </summary>
    public void RegisterInput(Component component)
    {
        Register(component, _input, _inputPaused);

        if (_input.Count == 1)
            _managerNode.SetProcessInput(true);
    }

    /// <summary>
    /// Unregisters a component from input processing.
    /// </summary>
    public void UnregisterInput(Component component)
    {
        Unregister(component, _input, _inputPaused);

        if (_input.Count == 0)
            _managerNode.SetProcessInput(false);
    }

    /// <summary>
    /// Registers a component for unhandled input processing.
    /// </summary>
    public void RegisterUnhandledInput(Component component)
    {
        Register(component, _unhandledInput, _unhandledInputPaused);

        if (_unhandledInput.Count == 1)
            _managerNode.SetProcessUnhandledInput(true);
    }

    /// <summary>
    /// Unregisters a component from unhandled input processing.
    /// </summary>
    public void UnregisterUnhandledInput(Component component)
    {
        Unregister(component, _unhandledInput, _unhandledInputPaused);

        if (_unhandledInput.Count == 0)
            _managerNode.SetProcessUnhandledInput(false);
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
