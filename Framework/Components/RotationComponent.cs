using Godot;

namespace GodotUtils;

// Useful to quickly rotate a Sprite2D node to see if the game is truly paused or not
[GlobalClass]
public partial class RotationComponent : Node
{
    [Export] private float _speed = 1.5f;

    private Node2D _parent;

    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
    }

    public override void _Process(double delta)
    {
        _parent.Rotation += _speed * (float)delta;
    }
}
