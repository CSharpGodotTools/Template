#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// The main core class for the visualizer UI
/// </summary>
internal static class VisualUI
{
    private const string MainPanelName = "Main Panel";
    private const string MutableMembersName = "Mutable Members";
    private const string ReadonlyMembersName = "Readonly Members";
    private const string MembersColumnsName = "Members Columns";
    private const string LogsName = "Logs";
    private const string MainVBoxName = "Main VBox";

    /// <summary>
    /// Creates the visual panel for a specified visual node.
    /// </summary>
    public static (Control, IReadOnlyList<Action>) CreateVisualPanel(VisualData visualData)
    {
        Node node = visualData.AnchorNode;
        object visualizedObject = visualData.VisualizedObject;
        string[] readonlyMembers =
        [
            .. visualData.Properties.Select(property => property.Name),
            .. visualData.Fields.Select(field => field.Name)
        ];

        PanelContainer panelContainer = VisualUiElementFactory.CreatePanelContainer(node.Name);
        panelContainer.MouseFilter = MouseFilterEnum.Ignore;
        panelContainer.Name = MainPanelName;

        Vector2 currentCameraZoom = GetCurrentCameraZoom(node);
        panelContainer.Scale = new Vector2(1f / currentCameraZoom.X, 1f / currentCameraZoom.Y) * VisualUiLayout.PanelScaleFactor;

        VBoxContainer mutableMembersVbox = VisualUiElementFactory.CreateColoredVBox(VisualUiResources.MutableMembersColor);
        mutableMembersVbox.MouseFilter = MouseFilterEnum.Ignore;
        mutableMembersVbox.Name = MutableMembersName;

        VBoxContainer readonlyMembersVbox = VisualUiElementFactory.CreateColoredVBox(VisualUiResources.ReadonlyMembersColor);
        readonlyMembersVbox.MouseFilter = MouseFilterEnum.Ignore;
        readonlyMembersVbox.Name = ReadonlyMembersName;

        // Readonly Members
        ReadonlyMemberBinder readonlyBinder = new();
        readonlyBinder.AddReadonlyControls(readonlyMembers, visualizedObject, readonlyMembersVbox, node.Name);

        // Mutable Members
        VisualMemberElementBuilder.AddMutableControls(mutableMembersVbox, visualData.Properties, visualizedObject, node.Name);
        VisualMemberElementBuilder.AddMutableControls(mutableMembersVbox, visualData.Fields, visualizedObject, node.Name);

        // Methods
        VisualMethods.AddMethodInfoElements(mutableMembersVbox, visualData.Methods, visualizedObject);

        VBoxContainer vboxLogs = new()
        {
            Name = LogsName
        };
        mutableMembersVbox.AddChild(vboxLogs);

        VisualizeAutoload.Instance?.RegisterLogContainer(node, vboxLogs);

        ScrollContainer scrollContainer = new()
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever,
            CustomMinimumSize = new Vector2(0, VisualUiLayout.MinScrollViewDistance)
        };

        VBoxContainer titleBar = VisualTitleBarBuilder.Build(node.Name, mutableMembersVbox, readonlyMembersVbox, visualData, readonlyMembers);
        titleBar.Name = MainVBoxName;
        titleBar.MouseFilter = MouseFilterEnum.Ignore;

        HBoxContainer membersColumns = new()
        {
            Name = MembersColumnsName,
            MouseFilter = MouseFilterEnum.Ignore
        };
        membersColumns.AddChild(readonlyMembersVbox);
        membersColumns.AddChild(mutableMembersVbox);
        titleBar.AddChild(membersColumns);

        scrollContainer.AddChild(titleBar);
        panelContainer.AddChild(scrollContainer);

        return (panelContainer, readonlyBinder.UpdateActions);
    }

    private static Vector2 GetCurrentCameraZoom(Node node)
    {
        Viewport viewport = node.GetViewport();
        if (viewport == null)
            return Vector2.One;

        Camera2D cam2D = viewport.GetCamera2D();

        return cam2D?.Zoom ?? Vector2.One;
    }
}
#endif
