using Godot;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Scene reference container exposed to the inspector.
/// </summary>
[GlobalClass]
public partial class Scenes : Node
{
    /// <summary>
    /// Main gameplay scene loaded when starting or resuming a game run.
    /// </summary>
    [Export] public PackedScene Game { get; private set; } = null!;
}
