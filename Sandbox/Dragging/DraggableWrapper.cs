using Godot;

namespace __TEMPLATE__;

// Abstract wrapper interface to handle property/method differences between Control and Node2D
public interface ICanvasItemWrapper<T> where T : Node
{
    T Node { get; init; }
    Vector2 Position { get; set; }
    Vector2 GlobalPosition { get; set; }
    Vector2 GetTarget();
    CollisionShape2D GetCollision();
}
