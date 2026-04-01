using Godot;
using GodotUtils;

namespace __TEMPLATE__;

/// <summary>
/// Forwards Godot lifecycle callbacks into the framework <see cref="ComponentManager"/>.
/// </summary>
public partial class GameComponentManager : Node
{
    /// <summary>
    /// Root component manager that orchestrates composed game components.
    /// </summary>
    private ComponentManager _componentManager = null!;

    // Godot Overrides
    public override void _EnterTree()
    {
        _componentManager = new ComponentManager(this);
        _componentManager.EnterTree();
    }

    public override void _Ready()
    {
        _componentManager.Ready();
    }

    public override void _Process(double delta)
    {
        _componentManager.Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _componentManager.PhysicsProcess(delta);
    }

    public override void _Input(InputEvent @event)
    {
        _componentManager.Input(@event);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        _componentManager.UnhandledInput(@event);
    }
}
