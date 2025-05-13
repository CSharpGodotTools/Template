using Godot;
using GodotUtils;

namespace Template;

public class DraggableNode2D(Node2D node, DraggableComponent component) 
    : Draggable<Node2DWrapper, Node2D>(new Node2DWrapper(node), component);


// Concrete wrapper implementation
public class Node2DWrapper(Node2D node) : ICanvasItemWrapper<Node2D>
{
    public Node2D Node { get; init; } = node;
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
        Node.GetGlobalMousePosition();
    
    public CollisionShape2D GetCollision() =>
        Node.GetNode<Area2D>(recursive: false).GetChild<CollisionShape2D>(0);
}