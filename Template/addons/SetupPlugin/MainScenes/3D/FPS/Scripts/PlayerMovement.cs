using Framework;
using Godot;
using GodotUtils;

namespace __TEMPLATE__.FPS;

public class PlayerMovement(Player player, PlayerMovementConfig config, Camera3D camera) : Component(player)
{
    private const float HalfPi = Mathf.Pi * 0.5f;

    private Vector3 _gravityVec;
    private float _yaw;
    private float _pitch;

    protected override void Ready()
    {
        SetPhysicsProcess(true);
        SetInput(true);
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

        // Rotate camera
        Quaternion yawQuat = new(Vector3.Up, _yaw);
        Quaternion pitchQuat = new(Vector3.Right, _pitch);

        camera.Transform = new Transform3D(new Basis(yawQuat * pitchQuat), camera.Transform.Origin);
    }

    protected override void ProcessInput(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            return;

        if (@event is InputEventMouseMotion motion)
        {
            float sensitivity = config.MouseSensitivity * 0.01f;

            _yaw -= motion.Relative.X * sensitivity;
            _pitch -= motion.Relative.Y * sensitivity;

            _pitch = Mathf.Clamp(_pitch, -HalfPi, HalfPi);
        }
    }
}
