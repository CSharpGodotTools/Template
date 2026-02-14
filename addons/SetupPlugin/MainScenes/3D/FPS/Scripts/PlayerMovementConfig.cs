using Godot;

namespace __TEMPLATE__.FPS;

[GlobalClass]
public partial class PlayerMovementConfig : Resource
{
    [Export] public float GravityForce { get; set; } = 10;
    [Export] public float JumpForce { get; set; } = 150;
    [Export] public float MoveSpeed { get; set; } = 10;
    [Export] public float MoveDampening { get; set; } = 20; // the higher the value, the less the player will slide
    [Export] public float MouseSensitivity { get; set; } = 0.3f;
}
