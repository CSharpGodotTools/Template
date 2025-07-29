using Godot;
using GodotUtils;

namespace __TEMPLATE__.DragManager;

public class DraggableControl(Control control, DraggableComponent component)
    : Draggable<ControlWrapper, Control>(new ControlWrapper(control), component)
{
    public void ResizeArea(Vector2 newSize)
    {
        CollisionShape2D collision = Item.GetCollision();
        collision.Shape = new RectangleShape2D { Size = newSize };
        collision.Position = newSize * 0.5f;
    }
}

// Concrete wrapper implementation
public class ControlWrapper(Control control) : ICanvasItemWrapper<Control>
{
    public Control Node { get; init; } = control;
    public Vector2 Position 
    { 
        get => Node.Position; 
        set => Node.Position = value; 
    }

    public Vector2 GlobalPosition 
    { 
        get => Node.GlobalPosition; 
        set => Node.GlobalPosition = value; 
    }

    public Vector2 GetTarget() => 
        Node.GetGlobalMousePosition() - Node.Size * 0.5f;

    public CollisionShape2D GetCollision() =>
        Node.GetNode<Area2D>(recursive: false).GetChild<CollisionShape2D>(0);
}
