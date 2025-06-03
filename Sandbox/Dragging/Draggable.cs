using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__;

// A unified draggable implementation that works with any CanvasItem
public abstract class Draggable<T, TNode>(T item, DraggableComponent component) : IDraggable
    where T : class, ICanvasItemWrapper<TNode>
    where TNode : Node
{
    public readonly T Item = item;
    
    private readonly Vector2 _offset = component.KeepOffset ? 
        item.GlobalPosition - item.GetTarget() : 
        Vector2.Zero;
    
    private readonly float _smooth = component.LerpFactor;
    
    private readonly CollisionShape2D _collision = item.GetCollision();

    #region POSITION
    
    public Vector2 Position 
    {
        get => Item.Position;
        set => Item.Position = value;
    }

    public Vector2 GlobalPosition
    {
        get => Item.GlobalPosition;
        set => Item.GlobalPosition = value;
    }
    #endregion
    
    public void AnimateDrop(Action finished, double duration)
    {
        new GTween(Item.Node)
            .SetAnimatingProp(Node2D.PropertyName.Position)
            .AnimateProp(Vector2.Zero, duration)
            .Callback(finished);
    }

    public void FollowCursor()
    {
        Vector2 target = Item.GetTarget() + _offset;
        float distance = GlobalPosition.DistanceTo(target);
        GlobalPosition = GlobalPosition.MoveToward(target, distance * _smooth);
    }

    public void Reparent(Node newParent) =>
        Item.Node.Reparent(newParent);

    public void SetCollisionActive(bool active) =>
        _collision.Disabled = !active;
}