using Godot;
using GodotUtils;

namespace __TEMPLATE__.TopDown2D;

[GlobalClass]
public partial class JumpState : EnemyState
{
    [Export] private double _jumpTime = 1.0;
    [Export] private string _jumpAnimationName = "jump";
    [Export] private string _idleAnimationName = "idle";
    [Export] private Area2D _hitbox;

    protected override void Enter()
    {
        if (!IsInstanceValid(EnemyComponent.Target))
        {
            SwitchState(NextState, false);
            return;
        }

        Sprite.Play(_jumpAnimationName);

        Vector2 force = (EnemyComponent.Target.Position - Entity.Position) * 2;

        Entity.ApplyCentralImpulse(force);

        _hitbox.SetCollisionLayerAndMask(2);

        float size = 1.5f;

        new GTween(Sprite)
            .SetAnimatingProp(Node2D.PropertyName.Scale)
            .AnimateProp(Sprite.Scale * new Vector2(size * 2, size), 0.2).EaseOut()
            .AnimateProp(Sprite.Scale * new Vector2(size, size * 2), _jumpTime * 0.5).EaseOut()
            .AnimateProp(Sprite.Scale * new Vector2(1.5f, 0.5f), _jumpTime * 0.5).EaseIn()
            .Callback(() =>
            {
                _hitbox.SetCollisionLayerAndMask(1);
                Entity.LinearVelocity = Vector2.Zero;
                Sprite.Play(_idleAnimationName);
            })
            .AnimateProp(Sprite.Scale * Vector2.One, _jumpTime * 0.5)
            .Callback(() => SwitchState(NextState));
    }
}
