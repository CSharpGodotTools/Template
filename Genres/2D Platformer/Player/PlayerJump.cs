using Godot;
using GodotUtils;

namespace __TEMPLATE__.Platformer2D.Retro;

public partial class Player
{
    private readonly PlayerJumpVars _jumpVars = new();

    private State Jump()
    {
        State state = new(nameof(Jump)) { 
            Enter = () =>
            {
                _jumpVars.HoldingKey = true;
                _jumpVars.LossBuildUp = 0;
                Velocity -= new Vector2(0, _jumpVars.Force);
            },
            Update = _ =>
            {
                if (Input.IsActionPressed(InputActions.Jump) && _jumpVars.HoldingKey)
                {
                    _jumpVars.LossBuildUp += _jumpVars.Loss;
                    Velocity -= new Vector2(
                        x: 0,
                        y: Mathf.Max(0, _jumpVars.Force - _jumpVars.LossBuildUp));
                }

                if (Input.IsActionJustReleased(InputActions.Jump))
                {
                    _jumpVars.HoldingKey = false;
                }

                // Transitions
                if (IsOnFloor())
                {
                    SwitchState(Idle());
                }
            }
        };

        return state;
    }

    public class PlayerJumpVars
    {
        public float Force { get; } = 100;
        public float Loss { get; } = 7.5f;
        public float LossBuildUp { get; set; }
        public bool HoldingKey { get; set; } // the jump key
    }
}
