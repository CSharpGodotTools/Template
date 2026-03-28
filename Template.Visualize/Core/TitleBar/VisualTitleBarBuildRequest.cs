#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal readonly record struct VisualTitleBarBuildRequest(
    string Name,
    Control MutableMembersVbox,
    Control ReadonlyMembersVbox,
    Control MethodsVbox,
    VisualData VisualData,
    string[] ReadonlyMembers);
#endif
