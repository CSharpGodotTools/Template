using Godot;

namespace __TEMPLATE__.TopDown2D;

[GlobalClass]
public partial class PlayerConfig : Resource
{
    [Export] public float Speed { get; set; } = 50;
    [Export] public float Friction { get; set; } = 0.2f;
    [Export] public float DashStrength { get; set; } = 1500;
    [Export] public float LookLerpSpeed { get; set; } = 5;
    [Export] public float ExternalForceDecay { get; set; } = 0.2f;
}
