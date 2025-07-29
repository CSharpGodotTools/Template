using Godot;

namespace __TEMPLATE__.TopDown2D;

[GlobalClass]
public partial class CameraShakeComponent : Node
{
    private CameraShakeConfig _config;
    private Camera2D          _camera;
    private double            _remainingTime;
    private double            _freqCounter;

    public override void _Ready()
    {
        _camera = GetParent<Camera2D>();
        _config = new();
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Constantly subtract from remaining time
        _remainingTime -= delta;
        _freqCounter += delta;

        // Update offset at frequency
        if (_freqCounter >= _config.Frequency)
        {
            _freqCounter = 0;
            _camera.Offset = new Vector2((GD.Randf() - 0.5f), (GD.Randf() - 0.5f)) * _config.Amplitude;
        }

        // Stop shaking and reset camera offset when remaining time reaches zero
        if (_remainingTime <= 0)
        {
            _camera.Offset = Vector2.Zero;
            SetPhysicsProcess(false);
        }
    }

    public void Start(CameraShakeConfig newConfig)
    {
        // Duration is accumulative
        _remainingTime += newConfig.Duration;

        // Only set new amplitude if it's greater than current amplitude
        if (newConfig.Amplitude > _config.Amplitude)
        {
            _config.Amplitude = newConfig.Amplitude;
        }

        // Update frequency
        _config.Frequency = newConfig.Frequency;

        // Start shaking
        SetPhysicsProcess(true);
    }
}

public class CameraShakeConfig
{
    public Vector2 Amplitude { get; set; }
    public double Duration { get; set; } = 1.0;
    public double Frequency { get; set; }
}
