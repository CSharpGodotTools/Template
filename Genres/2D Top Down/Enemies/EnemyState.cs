using Godot;
using GodotUtils;

namespace __TEMPLATE__.TopDown2D;

/// <summary>
/// This script assumes all enemies extend from RigidBody2D
/// </summary>
[GlobalClass]
public abstract partial class EnemyState : Node2D
{
    [Export] protected EnemyState NextState { get; private set; }

    public State State { get; private set; }

    protected RigidBody2D Entity { get; private set; }
    protected AnimatedSprite2D Sprite { get => EnemyComponent.AnimatedSprite; }
    protected EnemyComponent EnemyComponent { get; private set; }

    public override void _Ready()
    {
        EnemyComponent = GetParent<EnemyComponent>();

        Entity = GetOwner() as RigidBody2D;

        State = new State(GetType().Name.Replace("State", ""))
        {
            Enter = Enter,
            Update = Update,
            Exit = Exit
        };

        NextState ??= EnemyComponent.StateMachine.IdleState;
    }
    
    protected virtual void Enter() { }
    protected virtual void Update(double delta) { }
    protected virtual void Exit() { }

    protected bool IsState(string state)
    {
        return EnemyComponent.StateMachine.IsState(state);
    }

    protected void SwitchState(EnemyState state, bool callExit = true)
    {
        EnemyComponent.StateMachine.SwitchState(state, callExit);
    }
}
