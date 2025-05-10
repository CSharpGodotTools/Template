using Godot;
using GodotUtils;
using System;

namespace Template;

[GlobalClass]
public partial class DraggableNode : Node
{
    private Node _parent;

    private Node2D _parentNode2D;
    private Control _parentControl;
    
    private Action _follow;
    
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        
        _parent = GetParent();
        
        switch (_parent)
        {
            case Sprite2D sprite:
                _parentNode2D = sprite;
                _follow = FollowNode2D;
                DetectSprite(sprite);
                break;
            case AnimatedSprite2D sprite:
                _parentNode2D = sprite;
                _follow = FollowNode2D;
                DetectSprite(sprite);
                break;
            case Control control:
                _parentControl = control;
                _follow = FollowControl;
                DetectControl(control);
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _follow();
    }

    private void FollowNode2D()
    {
        _parentNode2D.Position = _parentNode2D.GetGlobalMousePosition();
    }

    private void FollowControl()
    {
        _parentControl.Position = _parentControl.GetGlobalMousePosition();
    }

    private void DetectControl(Control control)
    {
        control.GuiInput += (inputEvent) =>
        {
            if (IsLeftClick(inputEvent))
            {
                _parent.Reparent(GetViewport());
                SetPhysicsProcess(true);
            }
        };
    }

    private void DetectSprite(Node2D node)
    {
        Area2D area = CreateSpriteArea(node);
        
        // InputEvent still works if monitoring is disabled
        area.Monitorable = false;
        area.Monitoring = false;
        
        area.InputEvent += (viewport, inputEvent, id) =>
        {
            if (IsLeftClick(inputEvent))
            {
                _parent.Reparent(GetViewport());
                SetPhysicsProcess(true);
            }
        };
    }
    
    private static bool IsLeftClick(InputEvent @event)
    {
        if (@event is not InputEventMouseButton btn)
            return false;

        return btn.ButtonIndex == MouseButton.Left && btn.Pressed;
    }
    
    private static Area2D CreateSpriteArea(Node2D node)
    {
        CollisionShape2D shape = new();
        shape.Shape = new RectangleShape2D { Size = GetSpriteSize(node) };
        
        Area2D area = new();
        area.AddChild(shape);
        
        node.CallDeferred(Node.MethodName.AddChild, area);

        return area;
    }
    
    private static Vector2 GetSpriteSize(Node node)
    {
        return node switch
        {
            Sprite2D sprite => sprite.GetSize(),
            AnimatedSprite2D sprite => sprite.GetSize(),
            _ => Vector2.Zero
        };
    }
}
