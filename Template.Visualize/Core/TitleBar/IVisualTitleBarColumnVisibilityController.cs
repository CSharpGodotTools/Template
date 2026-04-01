#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Controls column visibility state for title-bar member and method sections.
/// </summary>
internal interface IVisualTitleBarColumnVisibilityController
{
    /// <summary>
    /// Updates visible state for member/method columns and related title-bar controls.
    /// </summary>
    /// <param name="mutableMembersVbox">Mutable members container.</param>
    /// <param name="readonlyMembersVbox">Readonly members container.</param>
    /// <param name="methodsVbox">Methods container.</param>
    /// <param name="title">Title label that may be updated based on visibility state.</param>
    /// <param name="mutableButton">Toggle button for mutable members.</param>
    /// <param name="readonlyButton">Toggle button for readonly members.</param>
    /// <param name="methodsButton">Toggle button for methods section.</param>
    void Update(
        Control mutableMembersVbox,
        Control readonlyMembersVbox,
        Control methodsVbox,
        Label title,
        Button? mutableButton,
        Button? readonlyButton,
        Button? methodsButton);
}
#endif
