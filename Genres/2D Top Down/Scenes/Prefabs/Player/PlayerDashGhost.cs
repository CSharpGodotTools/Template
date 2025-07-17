using Godot;
using GodotUtils;

namespace __TEMPLATE__.TopDown2D;

public partial class PlayerDashGhost : Node2D
{
    [OnInstantiate]
    private void Init(Vector2 position, AnimatedSprite2D spriteToClone)
    {
        Position = position;

        AnimatedSprite2D sprite = (AnimatedSprite2D)spriteToClone.Duplicate();
        sprite.Material = null;
        AddChild(sprite);
    }

    public override void _Ready()
    {
        Name = nameof(PlayerDashGhost);

        const double modulateDuration = 0.5;

        new GTween(this)
            .Animate(CanvasItem.PropertyName.Modulate, Colors.Transparent, modulateDuration).EaseOut()
            .Callback(QueueFree);
    }
}

