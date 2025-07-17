using Godot;
using System;

namespace __TEMPLATE__.TopDown2D;

[Icon(Images.GearIcon)]
public partial class PlayerInputs : Node
{
    public Vector2 FacingDirection { get; private set; } = Vector2.Down;

    public event Action OnDash;

    public override void _PhysicsProcess(double delta)
    {
        UpdateFacingDirection();
        HandleInputs();
    }

    private void HandleInputs()
    {
        if (Input.IsActionJustPressed(InputActions.Dash))
        {
            OnDash?.Invoke();
        }
    }

    private void UpdateFacingDirection()
    {
        Vector2 direction = PlayerUtils.GetDirection();

        if (direction != Vector2.Zero)
        {
            FacingDirection = direction;
        }
    }
}
