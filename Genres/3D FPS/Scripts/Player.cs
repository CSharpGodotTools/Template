namespace Template.FPS3D;

public partial class Player : CharacterBody3D
{
    [Export] OptionsManager options;
    [Export] Node3D fpsRig;
    [Export] BoneAttachment3D cameraBone;
    [Export] AnimationTree animTreeArms;
    [Export] AnimationTree animTreeGun;

    bool isReloading { get => animTreeArms.GetCondition("reload"); }

    float mouseSensitivity;
    float gravityForce = 10;
    float jumpForce = 150;
    float moveSpeed = 10;
    float moveDampening = 20; // the higher the value, the less the player will slide
    float blendSpaceAdsPosition;

    Camera3D camera;
    Vector2 mouseInput;
    Vector3 cameraTarget;
    Vector3 gravityVec;
    Vector3 camOffset;

    public override void _Ready()
    {
        camera = GetNode<Camera3D>("%Camera3D");
        camOffset = camera.Position - Position;

        mouseSensitivity = options.Options.MouseSensitivity * 0.0001f;

        UIOptionsGameplay gameplay = GetNode<UIPopupMenu>("%PopupMenu")
            .Options.GetNode<UIOptionsGameplay>("%Gameplay");

        gameplay.OnMouseSensitivityChanged += value =>
        {
            mouseSensitivity = value * 0.0001f;
        };
    }

    public override void _PhysicsProcess(double d)
    {
        float delta = (float)d;

        // Mouse motion
        Quaternion camTarget = Quaternion.FromEuler(cameraTarget);

        camera.Position = Position + camOffset;
        camera.Quaternion = (camTarget * GetAnimationRotations()).Normalized();

        fpsRig.Position = camera.Position;
        fpsRig.Quaternion = camTarget;

        float h_rot = camera.Basis.GetEuler().Y;

        float f_input = -Input.GetAxis("move_down", "move_up");
        float h_input = Input.GetAxis("move_left", "move_right");

        SetBlendSpace1DPosition("Rest", blendSpaceAdsPosition);

        if (Input.IsActionJustPressed("reload"))
        {
            SetAnimCondition("reload", true);
        }

        if (Input.IsActionPressed("ads"))
        {
            blendSpaceAdsPosition = blendSpaceAdsPosition.Lerp(1, 0.1f);
        }
        else
        {
            blendSpaceAdsPosition = blendSpaceAdsPosition.Lerp(0, 0.1f);
        }

        Vector3 dir = new Vector3(h_input, 0, f_input)
            .Rotated(Vector3.Up, h_rot) // Always face correct direction
            .Normalized(); // Prevent fast strafing movement

        if (IsOnFloor())
        {
            gravityVec = Vector3.Zero;

            if (Input.IsActionJustPressed("jump"))
            {
                gravityVec = Vector3.Up * jumpForce * delta;
            }
        }
        else
        {
            gravityVec += Vector3.Down * gravityForce * delta;
        }

        Velocity = Velocity.Lerp(dir * moveSpeed, moveDampening * delta);
        Velocity += gravityVec;

        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion motion || Input.MouseMode != Input.MouseModeEnum.Captured)
            return;

        mouseInput = motion.Relative;

        cameraTarget += new Vector3(
            -motion.Relative.Y * mouseSensitivity, 
            -motion.Relative.X * mouseSensitivity, 0);

        // Prevent camera from looking too far up or down
        Vector3 rotDeg = cameraTarget;
        rotDeg.X = Mathf.Clamp(rotDeg.X, -89f.ToRadians(), 89f.ToRadians());
        cameraTarget = rotDeg;
    }

    void SetAnimCondition(StringName path, bool v)
    {
        animTreeArms.SetCondition(path, v);
        animTreeGun.SetCondition(path, v);
    }

    void SetBlendSpace1DPosition(StringName path, float value)
    {
        animTreeArms.SetBlendSpace1DPosition(path, value);
        animTreeGun.SetBlendSpace1DPosition(path, value);
    }

    Quaternion GetAnimationRotations()
    {
        // The camera bone
        Quaternion camBoneQuat = new Quaternion(cameraBone.Basis);

        // Account for annoying offset from the camera bone
        Quaternion offset = Quaternion.FromEuler(new Vector3(-Mathf.Pi / 2, -Mathf.Pi, 0));

        // The end result (multiplying order matters and always normalize to prevent errors)
        return (camBoneQuat * offset).Normalized();
    }
}
