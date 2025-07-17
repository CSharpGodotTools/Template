using Godot;
using GodotUtils;

namespace __TEMPLATE__.TopDown2D;

public static class PuddleReflectionUtils
{
    public static void CreateReflection(Node entity, Node2D sprite)
    {
        Node2D reflection = (Node2D)sprite.Duplicate();

        reflection.Name = "Reflection";

        if (reflection is Sprite2D sprite2D)
        {
            reflection.Position = new Vector2(0, sprite2D.GetPixelHeight());
        }
        else if (reflection is AnimatedSprite2D reflectionAnimated)
        {
            AnimatedSprite2D original = sprite as AnimatedSprite2D;

            string[] animations = original.SpriteFrames.GetAnimationNames();
            
            if (animations.Length > 0)
            {
                original.Play(animations[0]);
            }

            reflection.Position = new Vector2(0, original.GetPixelHeight());

            original.AnimationChanged += () =>
            {
                reflectionAnimated.Play(original.Animation);
            };
        }

        reflection.Modulate = Color.Color8(255, 255, 255, 36);
        reflection.Scale = new Vector2(reflection.Scale.X, -reflection.Scale.Y);
        reflection.LightMask = GMath.GetLayerValues(5);
        reflection.ZIndex = -9;
        reflection.Material = new CanvasItemMaterial
        {
            LightMode = CanvasItemMaterial.LightModeEnum.LightOnly
        };

        entity.AddChildDeferred(reflection);
    }
}

