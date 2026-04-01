#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// Composes the runtime visualization panel for a tracked anchor node.
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
    /// Builds the full visualization panel and returns update actions for readonly controls.
    /// </summary>
    /// <param name="visualData">Anchor/object/member metadata for panel creation.</param>
    /// <returns>Panel root control and readonly update actions.</returns>
    public static (Control, IReadOnlyList<Action>) CreateVisualPanel(VisualData visualData)
    {
        Node node = visualData.AnchorNode;
        object visualizedObject = visualData.VisualizedObject;

        // Reuse property/field names for readonly mirror controls.
        string[] readonlyMembers =
        [
            .. visualData.Properties.Select(property => property.Name),
            .. visualData.Fields.Select(field => field.Name)
        ];

        // Root panel setup and camera-relative scaling.
        PanelContainer panelContainer = VisualUiElementFactory.CreatePanelContainer(node.Name);
        panelContainer.MouseFilter = MouseFilterEnum.Ignore;
        panelContainer.Name = MainPanelName;

        Vector2 currentCameraZoom = GetCurrentCameraZoom(node);
        panelContainer.Scale = new Vector2(1f / currentCameraZoom.X, 1f / currentCameraZoom.Y) * VisualUiLayout.PanelScaleFactor;

        // Create mutable and readonly member columns.
        VBoxContainer mutableMembersVbox = VisualUiElementFactory.CreateColoredVBox(VisualUiResources.MutableMembersColor);
        mutableMembersVbox.MouseFilter = MouseFilterEnum.Ignore;
        mutableMembersVbox.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        mutableMembersVbox.Name = MutableMembersName;

        VBoxContainer readonlyMembersVbox = VisualUiElementFactory.CreateColoredVBox(VisualUiResources.ReadonlyMembersColor);
        readonlyMembersVbox.MouseFilter = MouseFilterEnum.Ignore;
        readonlyMembersVbox.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        readonlyMembersVbox.Name = ReadonlyMembersName;

        // Populate readonly controls and collect live update callbacks.
        ReadonlyMemberBinder readonlyBinder = new();
        readonlyBinder.AddReadonlyControls(readonlyMembers, visualizedObject, readonlyMembersVbox, node.Name);

        // Populate editable member controls from properties and fields.
        VisualMemberElementBuilder.AddMutableControls(mutableMembersVbox, visualData.Properties, visualizedObject, node.Name);
        VisualMemberElementBuilder.AddMutableControls(mutableMembersVbox, visualData.Fields, visualizedObject, node.Name);

        VBoxContainer methodsVbox = new()
        {
            Name = MethodsName,
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };

        // Add method invocation controls.
        VisualMethods.AddMethodInfoElements(methodsVbox, visualData.Methods, visualizedObject);

        VBoxContainer vboxLogs = new()
        {
            Name = LogsName,
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin
        };

        // Register transient log container for this node.
        VisualizeAutoload.Instance?.RegisterLogContainer(node, vboxLogs);

        ScrollContainer scrollContainer = new()
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever,
            CustomMinimumSize = new Vector2(0, VisualUiLayout.MinScrollViewDistance)
        };

        // Build final stacked layout: title bar, members scroll, then logs.
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
            MouseFilter = MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Begin
        };
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

    /// <summary>
    /// Gets current camera zoom for panel scaling, falling back to identity zoom.
    /// </summary>
    /// <param name="node">Node used to resolve viewport and active 2D camera.</param>
    /// <returns>Camera zoom or <see cref="Vector2.One"/> when unavailable.</returns>
    private static Vector2 GetCurrentCameraZoom(Node node)
    {
        Viewport viewport = node.GetViewport();

        // Viewport can be unavailable during early lifecycle or teardown.
        if (viewport == null)
            return Vector2.One;

        Camera2D cam2D = viewport.GetCamera2D();

        return cam2D?.Zoom ?? Vector2.One;
    }
}
#endif
