using Godot;

namespace __TEMPLATE__.FPS3D;

public partial class Player
{
    private float _gravityForce = 10;
    private float _jumpForce = 150;
    private float _moveSpeed = 10;
    private float _moveDampening = 20; // the higher the value, the less the player will slide

    private Vector3 _gravityVec;

    private void OnPhysicsProcessMotion(double d)
    {
        MoveAndSlide();

        float delta = (float)d;
        float hRot = _camera.Basis.GetEuler().Y;

        float fInput = -Input.GetAxis(InputActions.MoveDown, InputActions.MoveUp);
        float hInput = Input.GetAxis(InputActions.MoveLeft, InputActions.MoveRight);

        Vector3 dir = new Vector3(hInput, 0, fInput)
            .Rotated(Vector3.Up, hRot) // Always face correct direction
            .Normalized(); // Prevent fast strafing movement

        if (IsOnFloor())
        {
            _gravityVec = Vector3.Zero;

            if (Input.IsActionJustPressed(InputActions.Jump))
            {
                _gravityVec = Vector3.Up * _jumpForce * delta;
            }
        }
        else
        {
            _gravityVec += Vector3.Down * _gravityForce * delta;
        }

        Velocity = Velocity.Lerp(dir * _moveSpeed, _moveDampening * delta);
        Velocity += _gravityVec;
    }
}
