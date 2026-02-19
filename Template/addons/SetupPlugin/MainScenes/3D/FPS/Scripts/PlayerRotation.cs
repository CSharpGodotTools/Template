using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.FPS;

public class PlayerRotation(Player player, PlayerMovementConfig config, Camera3D camera) : Component(player)
{
    public event Action<float> YawChanged;

    private const float HalfPi = Mathf.Pi * 0.5f;
    private float _yaw;
    private float _pitch;

    protected override void Ready()
    {
        SetPhysicsProcess(true);
        SetInput(true);
    }

    protected override void PhysicsProcess(double delta)
    {
        // Rotate camera
        Quaternion yawQuat = new(Vector3.Up, _yaw);
        Quaternion pitchQuat = new(Vector3.Right, _pitch);

        camera.Transform = new Transform3D(new Basis(yawQuat * pitchQuat), camera.Transform.Origin);
    }

    protected override void ProcessInput(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            return;

        if (@event is InputEventMouseMotion motion)
        {
            float sensitivity = config.MouseSensitivity * 0.01f;

            _yaw -= motion.Relative.X * sensitivity;
            _pitch -= motion.Relative.Y * sensitivity;

            _pitch = Mathf.Clamp(_pitch, -HalfPi, HalfPi);

            YawChanged?.Invoke(_yaw);
        }
    }
}
