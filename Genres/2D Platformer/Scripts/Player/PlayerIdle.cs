using Godot;
using GodotUtils;

namespace __TEMPLATE__.Platformer2D.Retro;

public partial class Player
{
    private State Idle()
    {
        State state = new(nameof(Idle)) 
        { 
            Enter = () =>
            {
                Sprite.Play("idle");
            },
            Update = _ =>
            {
                if (Input.IsActionJustPressed(InputActions.Jump) && IsOnFloor())
                {
                    SwitchState(Jump());
                }
                else if (Input.IsActionJustPressed(InputActions.MoveDown) && GetFloorAngle() > 0)
                {

                }
            }
        };

        return state;
    }
}

