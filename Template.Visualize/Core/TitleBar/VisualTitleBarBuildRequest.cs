#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Immutable input data required to build a visual title-bar control row.
/// </summary>
/// <param name="Name">Display title text.</param>
/// <param name="MutableMembersVbox">Mutable members column root.</param>
/// <param name="ReadonlyMembersVbox">Readonly members column root.</param>
/// <param name="MethodsVbox">Methods column root.</param>
/// <param name="VisualData">Visualized member and method metadata.</param>
/// <param name="ReadonlyMembers">Readonly member names list.</param>
internal readonly record struct VisualTitleBarBuildRequest(
    string Name,
    Control MutableMembersVbox,
    Control ReadonlyMembersVbox,
    Control MethodsVbox,
    VisualData VisualData,
    string[] ReadonlyMembers);
#endif
