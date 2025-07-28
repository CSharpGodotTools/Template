using Godot;
using GodotUtils;

namespace __TEMPLATE__.TopDown2D;

[GlobalClass]
public partial class ShakeState : EnemyState
{
    [Export] private double  _duration = 1;
    [Export] private double  _frequency = 0.05;
    [Export] private Vector2 _offset = new(2, 0.5f);
    [Export] private string  _animationName = "pre_jump";

    protected override void Enter()
    {
        GTween tweenShake = new(Sprite);
        Vector2 prevSpritePos = Sprite.Position;

        Sprite.Play(_animationName);

        tweenShake.SetAnimatingProp(Node2D.PropertyName.Position)
            .AnimateProp(Sprite.Position - _offset, _frequency)
            .AnimateProp(Sprite.Position + _offset, _frequency)
            .Loop();

        GTween.Delay(this, _duration, () =>
        {
            tweenShake.Stop();
            Sprite.Position = prevSpritePos;
            SwitchState(NextState);
        });
    }
}
