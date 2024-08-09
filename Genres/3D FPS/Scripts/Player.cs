namespace Template.FPS3D;

public partial class Player : CharacterBody3D
{
    [Export] OptionsManager options;
    [Export] Node3D fpsRig;
    [Export] BoneAttachment3D cameraBone;

    float mouseSensitivity;
    float gravityForce = 10;
    float jumpForce = 150;
    float moveSpeed = 10;
    float moveDampening = 20; // the higher the value, the less the player will slide
                                   
    Camera3D camera;
    Vector2 mouseInput;
    Vector3 cameraTarget;
    Vector3 gravityVec;
    Vector3 camOffset;

    AnimationPlayer animPlayerArms;
    AnimationPlayer animPlayerGun;

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

        animPlayerArms = (AnimationPlayer)fpsRig
            .GetNode("Arms")
            .FindChild("AnimationPlayer");

        animPlayerGun = (AnimationPlayer)fpsRig
            .GetNode("Gun")
            .FindChild("AnimationPlayer");
    }

    public override void _PhysicsProcess(double d)
    {
        float delta = (float)d;

        Quaternion camBoneQuat = new Quaternion(cameraBone.Basis);
        Quaternion camTarget = Quaternion.FromEuler(cameraTarget);
        Quaternion offset = Quaternion.FromEuler(new Vector3(-Mathf.Pi / 2, -Mathf.Pi, 0));
        Quaternion finalQuat = (camBoneQuat * offset * camTarget).Normalized();

        camera.Position = Position + camOffset;
        camera.Quaternion = finalQuat;

        float h_rot = camera.Basis.GetEuler().Y;

        float f_input = -Input.GetAxis("move_down", "move_up");
        float h_input = Input.GetAxis("move_left", "move_right");

        if (Input.IsActionJustPressed("reload"))
        {
            animPlayerArms.Play("arms_reload");
            animPlayerGun.Play("assault_rifle_reload");
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
}
