using Godot;
using GodotUtils;
using System;

namespace Template;

public class DraggableNode2D : IDraggable
{
    public Vector2 Position { get => _node.Position; set => _node.Position = value; }

    private readonly Vector2 _offset;
    private readonly Node2D _node;
    private readonly float _lerpFactor;
    private readonly CollisionShape2D _collision;

    public DraggableNode2D(Node2D node, DraggableComponent component)
    {
        _node = node;
        _lerpFactor = component.LerpFactor;
        _collision = node.GetNode<Area2D>(recursive: false).GetChild<CollisionShape2D>(0);

        if (component.KeepOffset)
        {
            _offset = _node.GlobalPosition - _node.GetGlobalMousePosition();
        }
    }

    public void AnimateDrop(Action finished, double duration)
    {
        new GTween(_node)
            .SetAnimatingProp(Node2D.PropertyName.Position)
            .AnimateProp(Vector2.Zero, duration)
            .Callback(finished);
    }
    
    public void FollowCursor()
    {
        // Following code might be the same as _node.GlobalPosition = _node.GlobalPosition.Lerp(_node.GetGlobalMousePosition() + _offset, _lerpFactor);
        Vector2 target = _node.GetGlobalMousePosition() + _offset;
        float distance = _node.GlobalPosition.DistanceTo(target);
        _node.GlobalPosition = _node.GlobalPosition.MoveToward(target, distance * _lerpFactor);
    }

    public void Reparent(Node newParent)
    {
        _node.Reparent(newParent);
    }

    public void SetCollisionActive(bool active)
    {
        _collision.Disabled = !active;
    }
}