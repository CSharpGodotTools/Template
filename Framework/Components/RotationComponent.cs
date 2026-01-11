using Godot;

namespace __TEMPLATE__;

// Useful to quickly rotate a Sprite2D node to see if the game is truly paused or not
[GlobalClass]
public partial class RotationComponent : Node
{
    #region Exports
    [Export] private float _speed = 1.5f;
    #endregion

    #region Variables
    private Node2D _parent;
    #endregion

    #region Godot Overrides
    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
    }

    public override void _Process(double delta)
    {
        _parent.Rotation += _speed * (float)delta;
    }
    #endregion
}
