using Godot;

namespace __TEMPLATE__;

/// <summary>
/// Rotates the parent <see cref="Node2D"/> each frame to provide a simple movement indicator.
/// </summary>
[GlobalClass]
public partial class RotationComponent : Node
{
    // Exports
    /// <summary>
    /// Rotation speed in radians per second.
    /// </summary>
    [Export] private float _speed = 1.5f;

    // Variables
    private Node2D _parent = null!;

    // Godot Overrides
    /// <inheritdoc />
    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _parent.Rotation += _speed * (float)delta;
    }
}
