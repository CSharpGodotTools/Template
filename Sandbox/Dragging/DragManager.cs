using Godot;
using GodotUtils;
using System.Collections.Generic;

namespace Template;

public partial class DragManager : Node2D
{
    private IDraggable _currentDraggable;
    private bool _animating;
    
    public override void _Ready()
    {
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        _currentDraggable.FollowCursor();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton btn)
            return;

        if (btn.IsLeftClickJustPressed() && _currentDraggable == null)
        {
            if (TryGetDragInfo(out DraggableComponent component, out Node item, out Area2D _))
            {
                Pickup(component, item);
            }
        }
        else if (btn.IsLeftClickJustReleased() && _currentDraggable != null && !_animating)
        {
            if (TryGetDragInfo(out DroppableComponent component, out Node container, out Area2D area))
            {
                DropInContainer(component, container, area);
            }
            else
            {
                StopTracking();
            }
        }
    }
    
    private void Pickup(DraggableComponent component, Node parent)
    {
        _currentDraggable = parent switch
        {
            Node2D node => new DraggableNode2D(node, component),
            Control control => new DraggableControl(control, component),
            _ => null
        };
        
        _currentDraggable?.Reparent(GetViewport());
            
        SetProcess(true);
    }

    private void DropInContainer(DroppableComponent component, Node container, Area2D area)
    {
        _currentDraggable.Reparent(container);

        switch (_currentDraggable)
        {
            // Drag Control onto Control
            case DraggableControl draggable when container is Control:
                ResizeAreaToContainer(draggable, area);
                
                if (!component.Animate)
                    _currentDraggable.Position = Vector2.Zero;
                break;
            
            // Drag Control onto Node2D
            case DraggableControl draggable when container is Node2D:
                if (!component.Animate)
                    _currentDraggable.Position = -draggable.Item.Node.Size * 0.5f;
                break;
            
            // Drag Node2D onto Control
            case DraggableNode2D _ when container is Control control:
                if (!component.Animate)
                    _currentDraggable.Position = control.Size * 0.5f;
                break;

            // Drag Control or Node2D onto nothing
            default:
                if (!component.Animate)
                    _currentDraggable.Position = Vector2.Zero;
                break;
        }

        if (component.Animate)
        {
            SetProcess(false);
            _animating = true;
            _currentDraggable.SetCollisionActive(false);
            _currentDraggable.AnimateDrop(() =>
            {
                _animating = false;
                _currentDraggable.SetCollisionActive(true);
                StopTracking();
            }, 0.2);
        }
        else
        {
            StopTracking();
        }
    }
    
    private void StopTracking()
    {
        SetProcess(false);
        _currentDraggable = null;
    }
    
    private bool TryGetDragInfo<T>(out T component, out Node parent, out Area2D area2D) where T : Node
    {
        area2D = null;
        component = null;
        parent = null;

        const int maxAreasToCheck = 2;
        List<Area2D> areas = CursorUtils2D.GetAreasUnderCursor(this, maxAreasToCheck);

        foreach (Area2D area in areas)
        {
            parent = area.GetParent();

            component = parent.GetNode<T>(recursive: false);

            if (parent == null || component == null)
                continue;

            area2D = area;
            return true;
        }

        return false;
    }
    
    private static void ResizeAreaToContainer(DraggableControl control, Area2D area)
    {
        control.ResizeArea(area.GetChild<CollisionShape2D>(0).Shape.GetRect().Size);
    }
}
