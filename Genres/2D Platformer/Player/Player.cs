using Godot;
using GodotUtils;

namespace __TEMPLATE__.Platformer2D.Retro;

public partial class Player : Entity
{
    private float _maxSpeed     = 500;
    private float _acceleration = 40;
    private float _friction     = 20;
    private float _gravity      = 20;

    public override void Update()
    {
        Vector2 vel = Velocity;
        float horzDir = Input.GetAxis("move_left", "move_right");

        // Horizontal movement
        vel.X += horzDir * _acceleration;
        vel.X = Utils.ClampAndDampen(vel.X, _friction, _maxSpeed);

        // Gravity
        vel.Y += _gravity;

        Velocity = vel;
    }

    protected override State InitialState()
    {
        return Idle();
    }
}
