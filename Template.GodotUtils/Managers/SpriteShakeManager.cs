using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Manages shaking for Sprite2D and AnimatedSprite2D nodes.
/// </summary>
public class SpriteShakeManager
{
    private readonly List<AnimatedSprite2D> _animatedSprites = [];
    private readonly List<Sprite2D> _sprites = [];
    private readonly RandomNumberGenerator _rng;

    /// <summary>
    /// Creates a manager for the provided sprite nodes.
    /// </summary>
    public SpriteShakeManager(List<Node2D> sprites)
    {
        ArgumentNullException.ThrowIfNull(sprites);

        foreach (Node2D node in sprites)
        {
            ArgumentNullException.ThrowIfNull(node);

            if (node is Sprite2D sprite)
            {
                _sprites.Add(sprite);
            }
            else if (node is AnimatedSprite2D animatedSprite)
            {
                _animatedSprites.Add(animatedSprite);
            }
            else
            {
                throw new ArgumentException($"Node '{node.Name}' is not a {nameof(Sprite2D)} or {nameof(AnimatedSprite2D)}");
            }
        }

        _rng = new RandomNumberGenerator();
        _rng.Randomize();
    }

    /// <summary>
    /// Applies a vertical shake offset with the provided intensity.
    /// </summary>
    public void Shake(float intensity)
    {
        float randomOffset = _rng.RandfRange(-intensity, intensity);

        SetVerticalOffset(randomOffset);
    }

    /// <summary>
    /// Resets sprite offsets to zero.
    /// </summary>
    public void Reset()
    {
        SetVerticalOffset(0);
    }

    private void SetVerticalOffset(float offsetY)
    {
        foreach (Sprite2D sprite in _sprites)
        {
            sprite.Offset = new Vector2(sprite.Offset.X, offsetY);
        }

        foreach (AnimatedSprite2D sprite in _animatedSprites)
        {
            sprite.Offset = new Vector2(sprite.Offset.X, offsetY);
        }
    }
}
