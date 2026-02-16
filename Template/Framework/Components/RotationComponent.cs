using Godot;

namespace Framework;

// Useful to quickly rotate a Sprite2D node to see if the game is truly paused or not
[GlobalClass]
public partial class RotationComponent : Node
{
    // Exports
    [Export] private float _speed = 1.5f;

    // Variables
    private Node2D _parent;

    // Godot Overrides
    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
    }

    public override void _Process(double delta)
    {
        _parent.Rotation += _speed * (float)delta;
    }
}
