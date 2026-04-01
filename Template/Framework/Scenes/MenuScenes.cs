using Godot;

namespace __TEMPLATE__.Ui;

[GlobalClass]
/// <summary>
/// Resource container that maps main menu destinations to PackedScene assets.
/// </summary>
public partial class MenuScenes : Resource
{
    /// <summary>
    /// Gets the main menu scene.
    /// </summary>
    [Export] public PackedScene MainMenu { get; private set; } = null!;

    /// <summary>
    /// Gets the mod loader scene.
    /// </summary>
    [Export] public PackedScene ModLoader { get; private set; } = null!;

    /// <summary>
    /// Gets the options scene.
    /// </summary>
    [Export] public PackedScene Options { get; private set; } = null!;

    /// <summary>
    /// Gets the credits scene.
    /// </summary>
    [Export] public PackedScene Credits { get; private set; } = null!;
}
