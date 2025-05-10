using Godot;
using GodotUtils;
using System;

namespace Template;

[GlobalClass]
public partial class DraggableNode : Node
{
    private IDraggable _draggable;
    
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        
        switch (GetParent())
        {
            case Sprite2D sprite:
                _draggable = new DraggableNode2D(sprite);
                DetectSprite(sprite);
                break;
            case AnimatedSprite2D sprite:
                _draggable = new DraggableNode2D(sprite);
                DetectSprite(sprite);
                break;
            case Control control:
                _draggable = new DraggableControl(control);
                DetectControl(control);
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _draggable.Follow();
    }

    private void DetectControl(Control control)
    {
        control.GuiInput += (inputEvent) =>
        {
            if (IsLeftClick(inputEvent))
            {
                _draggable.Reparent(GetViewport());
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
                _draggable.Reparent(GetViewport());
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

public interface IDraggable
{
    void Follow();
    void Reparent(Node parent);
}

public class DraggableNode2D(Node2D node) : IDraggable
{
    private readonly Node2D _node = node;

    public void Follow()
    {
        _node.Position = _node.GetGlobalMousePosition();
    }

    public void Reparent(Node parent)
    {
        _node.Reparent(parent);
    }
}

public class DraggableControl(Control control) : IDraggable
{
    private readonly Control _control = control;

    public void Follow()
    {
        _control.Position = _control.GetGlobalMousePosition();
    }

    public void Reparent(Node parent)
    {
        _control.Reparent(parent);
    }
}
