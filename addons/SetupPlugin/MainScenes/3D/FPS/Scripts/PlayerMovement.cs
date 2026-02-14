using Framework;
using Godot;
using GodotUtils;

namespace __TEMPLATE__.FPS;

public class PlayerMovement(Player player, PlayerMovementConfig config, Camera3D camera) : Component(player)
{
    private const int LookUpDownLimit = 89;

    // Movement
    private Vector3 _cameraTarget;
    private Vector2 _mouseInput;
    private Vector3 _gravityVec;

    protected override void Ready()
    {
        SetPhysicsProcess(true);
        SetInput(true);
    }

    protected override void PhysicsProcess(double d)
    {
        player.MoveAndSlide();

        float delta = (float)d;
        float hRot = camera.Basis.GetEuler().Y;

        float fInput = -Input.GetAxis(InputActions.MoveDown, InputActions.MoveUp);
        float hInput = Input.GetAxis(InputActions.MoveLeft, InputActions.MoveRight);

        Vector3 dir = new Vector3(hInput, 0, fInput)
            .Rotated(Vector3.Up, hRot) // Always face correct direction
            .Normalized(); // Prevent fast strafing movement

        if (player.IsOnFloor())
        {
            _gravityVec = Vector3.Zero;

            if (Input.IsActionJustPressed(InputActions.Jump))
            {
                _gravityVec = Vector3.Up * config.JumpForce * delta;
            }
        }
        else
        {
            _gravityVec += Vector3.Down * config.GravityForce * delta;
        }

        player.Velocity = player.Velocity.Lerp(dir * config.MoveSpeed, config.MoveDampening * delta);
        player.Velocity += _gravityVec;

        camera.Rotation = _cameraTarget * delta;
    }

    protected override void ProcessInput(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            return;

        if (@event is InputEventMouseMotion motion)
        {
            _mouseInput = motion.Relative;

            _cameraTarget += new Vector3(
                -motion.Relative.Y * config.MouseSensitivity,
                -motion.Relative.X * config.MouseSensitivity, 0);

            // Prevent camera from looking too far up or down
            Vector3 rotDeg = _cameraTarget;
            rotDeg.X = Mathf.Clamp(rotDeg.X, -LookUpDownLimit, LookUpDownLimit);
            _cameraTarget = rotDeg;
        }
    }
}
