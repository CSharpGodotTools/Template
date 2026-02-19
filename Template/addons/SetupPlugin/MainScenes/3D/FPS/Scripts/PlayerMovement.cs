using Framework;
using Godot;
using GodotUtils;

namespace __TEMPLATE__.FPS;

public class PlayerMovement(Player player, PlayerMovementConfig config, PlayerRotation playerRotation) : Component(player)
{
    private Vector3 _gravityVec;
    private float _yaw;

    protected override void Ready()
    {
        playerRotation.YawChanged += OnYawChanged;
        SetPhysicsProcess(true);
    }

    protected override void PhysicsProcess(double d)
    {
        float delta = (float)d;

        float fInput = -Input.GetAxis(InputActions.MoveDown, InputActions.MoveUp);
        float hInput = Input.GetAxis(InputActions.MoveLeft, InputActions.MoveRight);

        Vector3 dir = new Vector3(hInput, 0, fInput)
            .Rotated(Vector3.Up, _yaw)
            .Normalized();

        // Move player
        if (player.IsOnFloor())
        {
            _gravityVec = Vector3.Zero;

            if (Input.IsActionJustPressed(InputActions.Jump))
                _gravityVec = Vector3.Up * config.JumpForce * delta;
        }
        else
        {
            _gravityVec += Vector3.Down * config.GravityForce * delta;
        }

        player.Velocity = player.Velocity.Lerp(dir * config.MoveSpeed, config.MoveDampening * delta);
        player.Velocity += _gravityVec;

        player.MoveAndSlide();
    }

    protected override void ExitTree()
    {
        playerRotation.YawChanged -= OnYawChanged;
    }

    private void OnYawChanged(float yaw)
    {
        _yaw = yaw;
    }
}
