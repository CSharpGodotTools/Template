#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// The main core class for the visualizer UI
/// </summary>
internal static class VisualUI
{
    /// <summary>
    /// Creates the visual panel for a specified visual node.
    /// </summary>
    public static (Control, IReadOnlyList<Action>) CreateVisualPanel(VisualData visualData, string[] readonlyMembers)
    {
        Node node = visualData.Node;

        PanelContainer panelContainer = VisualUiElementFactory.CreatePanelContainer(node.Name);
        panelContainer.MouseFilter = MouseFilterEnum.Ignore;
        panelContainer.Name = "Main Panel";

        Vector2 currentCameraZoom = GetCurrentCameraZoom(node);
        panelContainer.Scale = new Vector2(1f / currentCameraZoom.X, 1f / currentCameraZoom.Y) * VisualUiLayout.PanelScaleFactor;

        VBoxContainer mutableMembersVbox = VisualUiElementFactory.CreateColoredVBox(VisualUiResources.MutableMembersColor);
        mutableMembersVbox.MouseFilter =  MouseFilterEnum.Ignore;
        mutableMembersVbox.Name = "Mutable Members";

        VBoxContainer readonlyMembersVbox = VisualUiElementFactory.CreateColoredVBox(VisualUiResources.ReadonlyMembersColor);
        readonlyMembersVbox.MouseFilter = MouseFilterEnum.Ignore;
        readonlyMembersVbox.Name = "Readonly Members";

        // Readonly Members
        ReadonlyMemberBinder readonlyBinder = new();
        readonlyBinder.AddReadonlyControls(readonlyMembers, node, readonlyMembersVbox);

        // Mutable Members
        VisualMemberElementBuilder.AddMutableControls(mutableMembersVbox, visualData.Properties, node);
        VisualMemberElementBuilder.AddMutableControls(mutableMembersVbox, visualData.Fields, node);

        // Methods
        VisualMethods.AddMethodInfoElements(mutableMembersVbox, visualData.Methods, node);

        VBoxContainer vboxLogs = new();
        vboxLogs.Name = "Logs";
        mutableMembersVbox.AddChild(vboxLogs);

        VisualizeAutoload.Instance?.RegisterLogContainer(node, vboxLogs);

        ScrollContainer scrollContainer = new()
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever,
            CustomMinimumSize = new Vector2(0, VisualUiLayout.MinScrollViewDistance)
        };

        // Make them hidden by default
        //mutableMembersVbox.Hide();
        //readonlyMembersVbox.Hide();

        VBoxContainer titleBar = VisualTitleBarBuilder.Build(node.Name, mutableMembersVbox, readonlyMembersVbox, visualData, readonlyMembers);
        titleBar.Name = "Main VBox";
        titleBar.MouseFilter = MouseFilterEnum.Ignore;
        titleBar.AddChild(readonlyMembersVbox);
        titleBar.AddChild(mutableMembersVbox);

        scrollContainer.AddChild(titleBar);
        panelContainer.AddChild(scrollContainer);
        
        // Add to canvas layer so UI is not affected by lighting in game world
        CanvasLayer canvasLayer = VisualUiElementFactory.CreateCanvasLayer(node.Name, node.GetInstanceId());
        canvasLayer.AddChild(panelContainer);

        node.CallDeferred(Node.MethodName.AddChild, canvasLayer);

        return (panelContainer, readonlyBinder.UpdateActions);
    }

    private static Vector2 GetCurrentCameraZoom(Node node)
    {
        Viewport viewport = node.GetViewport();
        if (viewport == null)
        {
            return Vector2.One;
        }

        Camera2D cam2D = viewport.GetCamera2D();

        if (cam2D != null)
        {
            return cam2D.Zoom;
        }

        return Vector2.One;
    }

}
#endif
