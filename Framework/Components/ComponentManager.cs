using Godot;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Using any kind of Godot functions from C# is expensive, so we try to minimize this with centralized logic.
/// See <see href="https://www.reddit.com/r/godot/comments/1me7669/a_follow_up_to_my_first_c_stress_test/">stress test results</see>.
/// </summary>
public partial class ComponentManager : Node
{
    private List<Component> _process = [];
    private List<Component> _physicsProcess = [];
    private List<Component> _unhandledInput = [];
    private List<Component> _input = [];

    // Disable overrides on startup for performance
    public override void _EnterTree()
    {
        //SetProcess(false); // Assume there will always be at least one process
        //SetPhysicsProcess(false); // Assume there will always be at least one physics process
        SetProcessInput(false);
        SetProcessUnhandledInput(false);
    }

    // Handle Godot overrides
    public override void _Process(double delta)
    {
        foreach (Component component in _process)
        {
            component.Process(delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Component component in _physicsProcess)
        {
            component.PhysicsProcess(delta);
        }
    }

    public override void _Input(InputEvent @event)
    {
        foreach (Component component in _input)
        {
            component.ProcessInput(@event);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        foreach (Component component in _unhandledInput)
        {
            component.UnhandledInput(@event);
        }
    }

    // Exposed register functions
    public void RegisterProcess(Component component)
    {
        if (_process.Contains(component))
            return;

        _process.Add(component);

        // Assume there will always be at least one process
        //if (_process.Count == 1)
        //    SetProcess(true);
    }

    public void UnregisterProcess(Component component)
    {
        _process.Remove(component);

        // Assume there will always be at least one process
        //if (_process.Count == 0)
        //    SetProcess(false);
    }

    public void RegisterPhysicsProcess(Component component)
    {
        if (_physicsProcess.Contains(component))
            return;

        _physicsProcess.Add(component);

        // Assume there will always be at least one physics process
        //if (_physicsProcess.Count == 1)
        //    SetPhysicsProcess(true);
    }

    public void UnregisterPhysicsProcess(Component component)
    {
        _physicsProcess.Remove(component);

        // Assume there will always be at least one physics process
        //if (_physicsProcess.Count == 0)
        //    SetPhysicsProcess(false);
    }

    public void RegisterInput(Component component)
    {
        if (_input.Contains(component))
            return;

        _input.Add(component);

        if (_input.Count == 1)
            SetProcessInput(true);
    }

    public void UnregisterInput(Component component)
    {
        _input.Remove(component);

        if (_input.Count == 0)
            SetProcessInput(false);
    }

    public void RegisterUnhandledInput(Component component)
    {
        if (_unhandledInput.Contains(component))
            return;

        _unhandledInput.Add(component);

        if (_unhandledInput.Count == 1)
            SetProcessUnhandledInput(true);
    }

    public void UnregisterUnhandledInput(Component component)
    {
        _unhandledInput.Remove(component);

        if (_unhandledInput.Count == 0)
            SetProcessUnhandledInput(false);
    }

    public void UnregisterAll(Component component)
    {
        UnregisterProcess(component);
        UnregisterPhysicsProcess(component);
        UnregisterInput(component);
        UnregisterUnhandledInput(component);
    }
}
