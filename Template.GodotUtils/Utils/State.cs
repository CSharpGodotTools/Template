using System;

namespace GodotUtils;

public class State(string name = "")
{
    private static readonly Action _noop = static () => { };
    private static readonly Action<double> _noopUpdate = static _ => { };

    public Action Enter { get; set; } = _noop;
    public Action<double> Update { get; set; } = _noopUpdate;
    public Action Exit { get; set; } = _noop;

    private readonly string _name = name;
    
    /// <summary>
    /// Returns the state name in lowercase.
    /// </summary>
    public override string ToString() => _name.ToLower();
}

public interface IStateMachine
{
    /// <summary>
    /// Switches to the provided state.
    /// </summary>
    void SwitchState(State newState);
}
