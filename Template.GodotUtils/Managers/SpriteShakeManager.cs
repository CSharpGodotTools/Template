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
    /// <param name="sprites">Sprites that receive shake offsets.</param>
    public SpriteShakeManager(List<Node2D> sprites)
    {
        ArgumentNullException.ThrowIfNull(sprites);

        foreach (Node2D node in sprites)
        {
            ArgumentNullException.ThrowIfNull(node);

            // Collect Sprite2D nodes for shake updates.
            if (node is Sprite2D sprite)
            {
                _sprites.Add(sprite);
            }
            // Collect AnimatedSprite2D nodes for shake updates.
            else if (node is AnimatedSprite2D animatedSprite)
            {
                _animatedSprites.Add(animatedSprite);
            }
            // Reject unsupported node types to keep shake updates type-safe.
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
    /// <param name="intensity">Maximum absolute vertical offset to apply.</param>
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

    /// <summary>
    /// Applies the same vertical offset to all tracked sprites while preserving horizontal offset.
    /// </summary>
    /// <param name="offsetY">Vertical offset to apply.</param>
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
