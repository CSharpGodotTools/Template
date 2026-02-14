using Framework;
using Godot;
using GodotUtils;
using System;
using PopupMenu = Framework.UI.PopupMenu;

namespace __TEMPLATE__;

public partial class Player : CharacterBody3D
{
    [Export] private Camera3D _camera;
    [Export] private PopupMenu _popupMenu;

    private ComponentHost _components = new();

    public override void _Ready()
    {
        _components.Add(new PlayerMovement(this, _camera));
        _components.Add(new PlayerMouseCapture(this, _popupMenu));
    }
}

public class PlayerMouseCapture(Player player, PopupMenu popupMenu) : Component(player)
{
    protected override void Ready()
    {
        CaptureCursor();

        popupMenu.Opened += OnPopupMenuOpened;
        popupMenu.Closed += OnPopupMenuClosed;
    }

    protected override void OnDispose()
    {
        popupMenu.Opened -= OnPopupMenuOpened;
        popupMenu.Closed -= OnPopupMenuClosed;
    }

    private void OnPopupMenuClosed()
    {
        CaptureCursor();
    }

    private void OnPopupMenuOpened()
    {
        ShowCursor();
    }

    private void CaptureCursor() => Input.MouseMode = Input.MouseModeEnum.Captured;
    private void ShowCursor() => Input.MouseMode = Input.MouseModeEnum.Visible;
}

public class PlayerMovement(Player player, Camera3D camera) : Component(player)
{
    // Config
    private float _gravityForce = 10;
    private float _jumpForce = 150;
    private float _moveSpeed = 10;
    private float _moveDampening = 20; // the higher the value, the less the player will slide

    private Vector3 _gravityVec;

    protected override void Ready()
    {
        SetPhysicsProcess(true);
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
                _gravityVec = Vector3.Up * _jumpForce * delta;
            }
        }
        else
        {
            _gravityVec += Vector3.Down * _gravityForce * delta;
        }

        player.Velocity = player.Velocity.Lerp(dir * _moveSpeed, _moveDampening * delta);
        player.Velocity += _gravityVec;
    }
}
