using Godot;
using GodotUtils;
using System;

namespace Template.Inventory;

[SceneTree]
public partial class AnimHelperItemContainer : ItemContainer
{
    public event Action OnReachedTarget;

    public Vector2 Target { get; set; }
    public bool TargetingMouse { get; set; }

    public float StartingLerp { get; set; } = 0.15f;
    private const float LerpToOneWeight = 0.01f;

    private float _currentLerp;

    [OnInstantiate]
    private void Init() { }

    public override void _Ready()
    {
        // Always QueueFree after so many seconds have passed
        new GTween(this).Delay(0.5).Callback(() =>
        {
            QueueFree();
            OnReachedTarget?.Invoke();
        });

        _currentLerp = StartingLerp;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (TargetingMouse)
        {
            Target = GetGlobalMousePosition() - CustomMinimumSize * 0.5f;
        }

        GlobalPosition = GlobalPosition.Lerp(Target, _currentLerp);
        
        if (GlobalPosition.DistanceTo(Target) < 1)
        {
            QueueFree();
            OnReachedTarget?.Invoke();
        }

        _currentLerp = Mathf.Lerp(_currentLerp, 1.0f, LerpToOneWeight);
    }

    public class Builder(AnimHelperItemContainer container)
    {
        public Builder SetInitialPositionForControl(Vector2 position)
        {
            container.GlobalPosition = position;
            return this;
        }

        public Builder SetInitialPositionForNode2D(Vector2 position)
        {
            container.GlobalPosition = position - container.CustomMinimumSize * 0.5f;
            return this;
        }

        public Builder SetControlTarget(Vector2 target)
        {
            container.Target = target;
            return this;
        }

        public Builder SetTargetAsMouse()
        {
            container.TargetingMouse = true;
            return this;
        }

        public Builder SetItemAndFrame(ItemStack item, int frame)
        {
            container.SetItem(item);
            container.SetCurrentSpriteFrame(frame);
            return this;
        }

        public Builder SetCount(int count)
        {
            container.SetCount(count);
            return this;
        }

        public Builder SetStartingLerp(float startingLerp)
        {
            container.StartingLerp = startingLerp;
            return this;
        }

        public AnimHelperItemContainer Build()
        {
            return container;
        }
    }
}
