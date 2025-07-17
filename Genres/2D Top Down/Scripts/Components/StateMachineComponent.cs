using Godot;
using GodotUtils;

namespace __TEMPLATE__.TopDown2D;

[GlobalClass, Icon(Images.GearIcon)]
public sealed partial class StateMachineComponent : Node2D
{
    [Export] public EnemyState IdleState { get; private set; }

    private State _curState;

    public override void _Ready()
    {
        _curState = IdleState.State;
        _curState.Enter();
    }

    public override void _PhysicsProcess(double delta)
    {
        _curState.Update((float)delta);
    }

    public void SwitchState(EnemyState newState, bool callExit = true)
    {
        SwitchState(newState.State, callExit);
    }

    public bool IsState(string state)
    {
        return string.Equals(_curState.ToString(), state, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Switches the current state of the object to a new state.
    /// </summary>
    /// <param name="newState">The new state to switch to.</param>
    /// <param name="callExit">A boolean indicating whether to call the Exit method on the current state before switching. Default is true.</param>
    private void SwitchState(State newState, bool callExit = true)
    {
        if (callExit)
        {
            _curState.Exit();
        }

        _curState = newState;
        _curState.Enter();
    }
}
