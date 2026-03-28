#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal static class VisualTitleBarBuilder
{
    public static VBoxContainer Build(string name, Control mutableMembersVbox, Control readonlyMembersVbox, Control methodsVbox, VisualData visualData, string[] readonlyMembers)
    {
        VisualTitleBarBuildRequest request = new(
            name,
            mutableMembersVbox,
            readonlyMembersVbox,
            methodsVbox,
            visualData,
            readonlyMembers);

        VisualTitleBarControlComponent component = CreateComponent();
        return component.Build(request);
    }

    private static VisualTitleBarControlComponent CreateComponent()
    {
        IVisualTitleBarAnchorPopupBuilder anchorPopupBuilder = new VisualTitleBarAnchorPopupBuilder();
        IVisualTitleBarColumnVisibilityController columnVisibilityController = new VisualTitleBarColumnVisibilityController();
        IVisualTitleBarValueSyncService valueSyncService = new VisualTitleBarValueSyncService();
        return new VisualTitleBarControlComponent(anchorPopupBuilder, columnVisibilityController, valueSyncService);
    }
}
#endif
