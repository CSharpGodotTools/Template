using Godot;
using GodotUtils;

namespace __TEMPLATE__.DragManager;

public static class DragUtils
{
    public static void AddChildArea(Node node)
    {
        Vector2 size = GetSize(node);

        CollisionShape2D shape = new()
        {
            Shape = new RectangleShape2D { Size = size }
        };

        Area2D area = new()
        {
            Monitorable = false,
            Monitoring = false
        };

        area.AddChild(shape);

        if (node is Control)
        {
            shape.Position += size * 0.5f;
        }
        
        node.CallDeferred(Node.MethodName.AddChild, area);
    }
    
    public static Vector2 GetSize(Node node)
    {
        return node switch
        {
            Sprite2D sprite => sprite.GetSize(),
            AnimatedSprite2D sprite => sprite.GetSize(),
            Control control => control.GetSize(),
            _ => Vector2.Zero
        };
    }
}