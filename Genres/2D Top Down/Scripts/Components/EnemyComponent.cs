using Godot;
using GodotUtils;
using System.Collections.Generic;

namespace __TEMPLATE__.TopDown2D;

/// <summary>
/// This script assumes all enemies are of type Node2D and have animated sprite nodes.
/// </summary>
[GlobalClass, Icon(Images.GearIcon)]
public partial class EnemyComponent : EntityComponent
{
    [Export] public StateMachineComponent StateMachine { get; private set; }
    [Export] private Area2D _hitbox;
    [Export] private float _damageInterval = 0.5f; // Time interval between damage ticks

    private Timer _damageTimer;
    private readonly HashSet<PlayerComponent> _playersInHitbox = [];

    public Node2D Target { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Services.Get<Level>().EnemyComponents.Add(this);

        _hitbox.BodyEntered += OnBodyEntered;
        _hitbox.BodyExited += OnBodyExited;

        // Initialize the timer
        _damageTimer = new Timer
        {
            WaitTime = _damageInterval,
            OneShot = false
        };

        _damageTimer.Timeout += OnDamageTimerTimeout;
        AddChild(_damageTimer);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.TryGetNode(out PlayerComponent playerComponent))
        {
            _playersInHitbox.Add(playerComponent);

            Vector2 direction = (playerComponent.GlobalPosition - GlobalPosition).Normalized();

            playerComponent.TakeDamage(direction); // Apply damage immediately

            if (_playersInHitbox.Count == 1)
            {
                _damageTimer.Start();
            }
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.TryGetNode(out PlayerComponent playerComponent))
        {
            _playersInHitbox.Remove(playerComponent);

            if (_playersInHitbox.Count == 0)
            {
                _damageTimer.Stop();
            }
        }
    }

    private void OnDamageTimerTimeout()
    {
        foreach (PlayerComponent player in _playersInHitbox)
        {
            Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();

            player.TakeDamage(direction); // Apply periodic damage
        }
    }
}
