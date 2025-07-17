using Godot;

namespace __TEMPLATE__.TopDown2D;

[Tool]
[SceneTree]
public partial class Torch : Node2D
{
    [Visualize] [Export(PropertyHint.Range, "0.01, 0.1, 0.01")] 
    private double _flickerRange = 0.05;

    [Visualize] [Export(PropertyHint.Range, "0.01, 0.4, 0.01")] 
    private double _pulseAmplitude = 0.1;

    [Visualize] [Export]
    private float TextureScale
    {
        get => _textureScale;
        set
        {
            if (_light != null)
            {
                _textureScale = value;
                _light.TextureScale = _textureScale;
            }
        }
    }
    
    [Visualize]
    private Vector2 Pos 
    {
        get => Position;
        set => Position = value;
    }

    [Visualize] private double _energy = 1;
    private float _textureScale = 1;
    private PointLight2D _light;

    public override void _Ready()
    {
        _light = _.PointLight2D;
    }

    public override void _PhysicsProcess(double delta)
    {
        _light.Energy = (float)(_energy + GD.RandRange(0, _flickerRange) - 
            Mathf.Sin(Engine.GetPhysicsFrames() * 0.01) * _pulseAmplitude);
    }
}
