using System;

namespace GodotUtils;

/// <summary>
/// Lightweight state container with enter, update, and exit callbacks.
/// </summary>
/// <param name="name">Optional state name used for diagnostics.</param>
public class State(string name = "")
{
    private static readonly Action _noop = static () => { };
    private static readonly Action<double> _noopUpdate = static _ => { };

    /// <summary>
    /// Called when the state becomes active.
    /// </summary>
    public Action Enter { get; set; } = _noop;

    /// <summary>
    /// Called each frame while the state is active.
    /// </summary>
    public Action<double> Update { get; set; } = _noopUpdate;

    /// <summary>
    /// Called when the state is exited.
    /// </summary>
    public Action Exit { get; set; } = _noop;

    private readonly string _name = name;

    /// <summary>
    /// Returns the state name in lowercase.
    /// </summary>
    /// <returns>Lowercase state name.</returns>
    public override string ToString() => _name.ToLower();
}

/// <summary>
/// Defines the minimal contract for state-machine transitions.
/// </summary>
public interface IStateMachine
{
    /// <summary>
    /// Switches to the provided state.
    /// </summary>
    /// <param name="newState">State to activate.</param>
    void SwitchState(State newState);
}
