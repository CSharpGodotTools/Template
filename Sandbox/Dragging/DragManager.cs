using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;

namespace Template;

public partial class DragManager : Node
{
    // Developers will hook into these delegates to define their logic
    public Func<Node, bool> CanDropInContainer { get; set; }
    public Func<bool> CanDrop { get; set; }
    
    // Members needed to keep track of current draggable and whether its being animated or not
    private IDraggable _currentDraggable;
    private bool _animating;

    // Keep track of previous parent and position
    private Node _prevParent;
    private Vector2 _prevPosition;
    
    public override void _Ready()
    {
        SetProcess(false);
    }

    // _Process is used because undesired teleportation effects can be noticed when using _PhysicsProcess (Tested using 180 Hz monitor)
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
                if (CanDropInContainer == null || CanDropInContainer(container))
                {
                    DropInContainer(component, container, area);
                }
                else
                {
                    ResetParentAndPosition();
                    StopTracking();
                }
            }
            else
            {
                if (CanDrop == null || CanDrop())
                {
                    StopTracking();
                }
                else
                {
                    ResetParentAndPosition();
                    StopTracking();
                }
            }
        }
    }
    
    private void Pickup(DraggableComponent component, Node item)
    {
        _currentDraggable = item switch
        {
            Node2D node => new DraggableNode2D(node, component),
            Control control => new DraggableControl(control, component),
            _ => null
        };

        _prevParent = item.GetParent();
        _prevPosition = _currentDraggable.Position;
        
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
    
    private void ResetParentAndPosition()
    {
        _currentDraggable.Reparent(_prevParent);
        _currentDraggable.Position = _prevPosition;
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
        List<Area2D> areas = CursorUtils2D.GetAreasUnderCursor(GetTree().Root.GetSceneNode<Node2D>(), maxAreasToCheck);

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
