using Godot;

namespace __TEMPLATE__.TopDown2D;

public partial class PlayerAnimation : Node
{
    [Export] private PlayerInputs _playerInputs;

    private AnimatedSprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetParent<AnimatedSprite2D>();
        _sprite.Play("idle_front");
    }

    public override void _PhysicsProcess(double delta)
    {
        Walking();
    }

    private void Walking()
    {
        Vector2 direction = PlayerUtils.GetDirection();

        if (direction.X != 0) // Moving left or right
        {
            _sprite.FlipH = direction.X < 0; // Flip sprite if moving left
            _sprite.Play("walk_side");
        }
        else if (direction.Y > 0) // Moving down
        {
            _sprite.Play("walk_front");
        }
        else if (direction.Y < 0) // Moving up
        {
            _sprite.Play("walk_back");
        }
        else // Not moving, idle animations
        {
            Idle();
        }
    }

    private void Idle()
    {
        if (_playerInputs.FacingDirection.X != 0)
        {
            _sprite.Play("idle_side");
        }
        else if (_playerInputs.FacingDirection == Vector2.Up)
        {
            _sprite.Play("idle_back");
        }
        else if (_playerInputs.FacingDirection == Vector2.Down)
        {
            _sprite.Play("idle_front");
        }
    }
}
