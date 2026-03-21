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
    private const string MethodsName = "Methods";
    private const string MembersColumnsName = "Members Columns";
    private const string LogsName = "Logs";
    private const string MainVBoxName = "Main VBox";
    private const int MembersColumnsSeparation = 6;

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
        mutableMembersVbox.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        mutableMembersVbox.Name = MutableMembersName;

        VBoxContainer readonlyMembersVbox = VisualUiElementFactory.CreateColoredVBox(VisualUiResources.ReadonlyMembersColor);
        readonlyMembersVbox.MouseFilter = MouseFilterEnum.Ignore;
        readonlyMembersVbox.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        readonlyMembersVbox.Name = ReadonlyMembersName;

        // Readonly Members
        ReadonlyMemberBinder readonlyBinder = new();
        readonlyBinder.AddReadonlyControls(readonlyMembers, visualizedObject, readonlyMembersVbox, node.Name);

        // Mutable Members
        VisualMemberElementBuilder.AddMutableControls(mutableMembersVbox, visualData.Properties, visualizedObject, node.Name);
        VisualMemberElementBuilder.AddMutableControls(mutableMembersVbox, visualData.Fields, visualizedObject, node.Name);

        VBoxContainer methodsVbox = new()
        {
            Name = MethodsName,
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };

        // Methods
        VisualMethods.AddMethodInfoElements(methodsVbox, visualData.Methods, visualizedObject);

        VBoxContainer vboxLogs = new()
        {
            Name = LogsName
        };
        vboxLogs.MouseFilter = MouseFilterEnum.Ignore;
        vboxLogs.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;

        VisualizeAutoload.Instance?.RegisterLogContainer(node, vboxLogs);

        ScrollContainer scrollContainer = new()
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever,
            CustomMinimumSize = new Vector2(0, VisualUiLayout.MinScrollViewDistance)
        };

        VBoxContainer mainLayout = new()
        {
            MouseFilter = MouseFilterEnum.Ignore
        };

        VBoxContainer titleBar = VisualTitleBarBuilder.Build(node.Name, mutableMembersVbox, readonlyMembersVbox, methodsVbox, visualData, readonlyMembers);
        titleBar.Name = MainVBoxName;
        titleBar.MouseFilter = MouseFilterEnum.Ignore;

        HBoxContainer membersColumns = new()
        {
            Name = MembersColumnsName,
            MouseFilter = MouseFilterEnum.Ignore
        };
        membersColumns.Alignment = BoxContainer.AlignmentMode.Begin;
        membersColumns.AddThemeConstantOverride("separation", MembersColumnsSeparation);
        membersColumns.AddChild(mutableMembersVbox);
        membersColumns.AddChild(readonlyMembersVbox);
        membersColumns.AddChild(methodsVbox);

        VBoxContainer scrollContent = new()
        {
            Name = "Scroll Content",
            MouseFilter = MouseFilterEnum.Ignore
        };
        scrollContent.AddChild(membersColumns);

        scrollContainer.AddChild(scrollContent);
        mainLayout.AddChild(titleBar);
        mainLayout.AddChild(scrollContainer);
        mainLayout.AddChild(vboxLogs);
        panelContainer.AddChild(mainLayout);

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
