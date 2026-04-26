#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Entry-point factory for constructing configured visual title-bar UI components.
/// </summary>
internal static class VisualTitleBarBuilder
{
    /// <summary>
    /// Builds a title-bar row with controls bound to provided visual data columns.
    /// </summary>
    /// <param name="name">Display title for the panel.</param>
    /// <param name="mutableMembersVbox">Mutable members column root.</param>
    /// <param name="readonlyMembersVbox">Readonly members column root.</param>
    /// <param name="methodsVbox">Methods column root.</param>
    /// <param name="visualData">Visualized member and method metadata.</param>
    /// <param name="readonlyMembers">Readonly member name list.</param>
    /// <returns>Configured title-bar container.</returns>
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

    /// <summary>
    /// Creates the concrete title-bar component with default service implementations.
    /// </summary>
    /// <returns>Configured title-bar control component.</returns>
    private static VisualTitleBarControlComponent CreateComponent()
    {
        IVisualTitleBarAnchorPopupBuilder anchorPopupBuilder = new VisualTitleBarAnchorPopupBuilder();
        IVisualTitleBarColumnVisibilityController columnVisibilityController = new VisualTitleBarColumnVisibilityController();
        IVisualTitleBarValueSyncService valueSyncService = new VisualTitleBarValueSyncService();
        return new VisualTitleBarControlComponent(anchorPopupBuilder, columnVisibilityController, valueSyncService);
    }
}
#endif
