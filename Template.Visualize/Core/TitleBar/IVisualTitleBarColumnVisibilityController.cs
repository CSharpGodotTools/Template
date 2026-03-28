#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal interface IVisualTitleBarColumnVisibilityController
{
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
