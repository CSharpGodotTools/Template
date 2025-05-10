using Godot;
using GodotUtils;
using System;

namespace Template;

public class DraggableControl : IDraggable
{
    public Control Control { get; }

    public Vector2 Position { get => Control.Position; set => Control.Position = value; }
    
    private readonly Vector2 _offset;
    private readonly float _smooth;
    private readonly CollisionShape2D _collision;

    public DraggableControl(Control control, DraggableComponent component)
    {
        Control = control;
        _smooth = component.LerpFactor;
        _collision = Control.GetNode<Area2D>(recursive: false).GetChild<CollisionShape2D>(0);

        if (component.KeepOffset)
        {
            _offset = Control.GlobalPosition - Control.GetGlobalMousePosition();
        }
    }
    
    public void AnimateDrop(Action finished, double duration)
    {
        new GTween(Control)
            .SetAnimatingProp(Node2D.PropertyName.Position)
            .AnimateProp(-Control.Size * 0.5f, duration)
            .Callback(finished);
    }

    public void FollowCursor()
    {
        // Following code might be the same as _node.GlobalPosition = _node.GlobalPosition.Lerp(_node.GetGlobalMousePosition() - Control.Size * 0.5f + _offset, _smooth);
        Vector2 target = Control.GetGlobalMousePosition() - Control.Size * 0.5f + _offset;
        float distance = Control.GlobalPosition.DistanceTo(target);
        Control.GlobalPosition = Control.GlobalPosition.MoveToward(target, distance * _smooth);
    }
    
    public void Reparent(Node newParent)
    {
        Control.Reparent(newParent);
    }
    
    public void ResizeArea(Vector2 newSize)
    {
        CollisionShape2D collision = Control.GetNode<Area2D>(recursive: false).GetChild<CollisionShape2D>(0);
        collision.Shape = new RectangleShape2D { Size = newSize };
        collision.Position = newSize * 0.5f;
    }
    
    public void SetCollisionActive(bool active)
    {
        _collision.Disabled = !active;
    }
}