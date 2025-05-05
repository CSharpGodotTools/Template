using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using static Godot.Control;
using GodotUtils;
using System.Linq;

namespace Template;

/// <summary>
/// The main core class for the visualizer UI
/// </summary>
public static class VisualUI
{
    public const float VISUAL_UI_SCALE_FACTOR = 0.6f;

    private const int MIN_SCROLL_VIEW_SIZE = 250;

    private static Texture2D _eyeOpen = GD.Load<Texture2D>("res://Template/Visualize/Icons/EyeOpen.png");
    private static Texture2D _eyeClosed = GD.Load<Texture2D>("res://Template/Visualize/Icons/EyeClosed.png");
    private static Texture2D _wrench = GD.Load<Texture2D>("res://Template/Visualize/Icons/Wrench.png");

    /// <summary>
    /// Creates the visual panel for a specified visual node.
    /// </summary>
    public static (Control, List<Action>) CreateVisualPanel(SceneTree tree, VisualData visualData)
    {
        Dictionary<Node, VBoxContainer> visualNodes = [];
        
        List<VisualSpinBox> spinBoxes = [];
        List<Action> updateControls = [];

        Node node = visualData.Node;

        PanelContainer panelContainer = CreatePanelContainer(node.Name);
        panelContainer.Name = "Main Panel";

        VBoxContainer mutableMembers = CreateColoredVBox(0.8f, 1, 0.8f);
        mutableMembers.Name = "Mutable Members";

        VBoxContainer readonlyMembers = CreateColoredVBox(1.0f, 0.75f, 0.8f);
        readonlyMembers.Name = "Readonly Members";

        // Readonly Members
        AddReadonlyControls(visualData.ReadonlyMembers, node, readonlyMembers, updateControls, spinBoxes);

        // Mutable Members
        AddMutableControls(mutableMembers, visualData.Properties, node, spinBoxes);
        AddMutableControls(mutableMembers, visualData.Fields, node, spinBoxes);

        // Methods
        VisualMethods.AddMethodInfoElements(mutableMembers, visualData.Methods, node, spinBoxes);

        VBoxContainer vboxLogs = new();
        vboxLogs.Name = "Logs";
        mutableMembers.AddChild(vboxLogs);

        visualNodes.Add(node, vboxLogs);

        ScrollContainer scrollContainer = new();
        scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever;
        scrollContainer.CustomMinimumSize = new Vector2(0, MIN_SCROLL_VIEW_SIZE);

        // Make them hidden by default
        mutableMembers.Hide();
        readonlyMembers.Hide();

        VBoxContainer titleBar = CreateTitleBar(node.Name, mutableMembers, readonlyMembers, visualData);
        titleBar.Name = "Main VBox";
        titleBar.AddChild(readonlyMembers);
        titleBar.AddChild(mutableMembers);

        SetButtonsToReleaseFocusOnPress(titleBar);

        scrollContainer.AddChild(titleBar);
        panelContainer.AddChild(scrollContainer);
        
        // Add to canvas layer so UI is not affected by lighting in game world
        CanvasLayer canvasLayer = CreateCanvasLayer(node.Name, node.GetInstanceId());
        canvasLayer.AddChild(panelContainer);

        node.CallDeferred(Node.MethodName.AddChild, canvasLayer);

        SetInitialPosition(mutableMembers, visualData.InitialPosition);

        // This is ugly but I don't know how else to do it
        Visualize.VisualNodes = visualNodes;

        return (panelContainer, updateControls);
    }
    
    /// <summary>
    /// Sets the initial position for a VBoxContainer.
    /// </summary>
    private static void SetInitialPosition(VBoxContainer vbox, Vector2 initialPosition)
    {
        if (initialPosition != Vector2.Zero)
        {
            vbox.GlobalPosition = initialPosition;
        }
    }

    /// <summary>
    /// Ensures buttons release focus on press within a VBoxContainer.
    /// </summary>
    private static void SetButtonsToReleaseFocusOnPress(VBoxContainer vboxParent)
    {
        foreach (BaseButton baseButton in vboxParent.GetChildren<BaseButton>())
        {
            baseButton.Pressed += () =>
            {
                _ = new GTween(baseButton)
                    .Delay(0.1)
                    .Callback(() => baseButton.ReleaseFocus());
            };
        }
    }

    private static VBoxContainer CreateTitleBar(string name, VBoxContainer mutableMembers, VBoxContainer readonlyMembers, VisualData visualData)
    {
        VBoxContainer vboxParent = new();

        HBoxContainer hbox = new()
        {
            Name = "Title Bar",
            Alignment = BoxContainer.AlignmentMode.Center
        };

        Label title = new()
        {
            Name = "Title",
            Text = name,
            Visible = false,
            LabelSettings = new LabelSettings
            {
                FontSize = 20,
                FontColor = Colors.LightSkyBlue,
                OutlineColor = Colors.Black,
                OutlineSize = 6,
            }
        };

        hbox.AddChild(title);

        Button readonlyBtn = null;
        Button mutableBtn = null;

        if (visualData.ReadonlyMembers != null)
        {
            readonlyBtn = CreateVisibilityButton(_eyeClosed, Colors.Pink);
            hbox.AddChild(readonlyBtn);
        }

        if (visualData.Properties.Any() || visualData.Fields.Any())
        {
            mutableBtn = CreateVisibilityButton(_wrench, Colors.Gray);
            hbox.AddChild(mutableBtn);
        }

        if (readonlyBtn != null)
        {
            readonlyBtn.Pressed += () =>
            {
                readonlyBtn.Icon = readonlyBtn.ButtonPressed ? _eyeOpen : _eyeClosed;
                readonlyMembers.Visible = readonlyBtn.ButtonPressed;
                title.Visible = readonlyBtn.ButtonPressed || (mutableBtn != null && mutableBtn.ButtonPressed);
            };
        }

        if (mutableBtn != null)
        {
            mutableBtn.Pressed += () =>
            {
                mutableMembers.Visible = mutableBtn.ButtonPressed;
                title.Visible = mutableBtn.ButtonPressed || (readonlyBtn != null && readonlyBtn.ButtonPressed);
            };
        }

        vboxParent.AddChild(hbox);

        return vboxParent;
    }

    private static Button CreateVisibilityButton(Texture2D icon, Color color)
    {
        Button btn = new()
        {
            Name = "Toggle Visibility",
            ToggleMode = true,
            Icon = icon,
            Flat = true,
            ExpandIcon = true,
            SelfModulate = color,
            CustomMinimumSize = Vector2.One * 25,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };

        btn.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());

        return btn;
    }

    /// <summary>
    /// Creates a colored VBoxContainer with specified RGB values.
    /// </summary>
    private static VBoxContainer CreateColoredVBox(float r, float g, float b)
    {
        return new VBoxContainer
        {
            Modulate = new Color(r, g, b, 1)
        };
    }
    
    /// <summary>
    /// Creates a panel container with a specified name.
    /// </summary>
    private static PanelContainer CreatePanelContainer(string name)
    {
        PanelContainer panelContainer = new()
        {
            // Ensure this info is rendered above all game elements
            Name = name,
            Scale = Vector2.One * VISUAL_UI_SCALE_FACTOR,
            ZIndex = (int)RenderingServer.CanvasItemZMax
        };

        panelContainer.AddThemeStyleboxOverride("panel", new StyleBoxEmpty());

        return panelContainer;
    }

    /// <summary>
    /// Attempts to get member information for a Node.
    /// </summary>
    private static void TryGetMemberInfo(Node node, string visualMember, out PropertyInfo property, out FieldInfo field, out object initialValue)
    {
        property = node.GetType().GetProperty(visualMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        field = null;
        initialValue = null;

        if (property != null)
        {
            initialValue = property.GetValue(property.GetGetMethod(true).IsStatic ? null : node);
        }
        else
        {
            field = node.GetType().GetField(visualMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            if (field != null)
            {
                initialValue = field.GetValue(field.IsStatic ? null : node);
            }
        }
    }

    /// <summary>
    /// Adds visual controls for specified members of a Node.
    /// </summary>
    private static void AddReadonlyControls(string[] visualizeMembers, Node node, VBoxContainer readonlyMembers, List<Action> updateControls, List<VisualSpinBox> spinBoxes)
    {
        if (visualizeMembers == null)
        {
            return;
        }
        
        foreach (string visualMember in visualizeMembers)
        {
            TryGetMemberInfo(node, visualMember, out PropertyInfo property, out FieldInfo field, out object initialValue);

            // If neither property nor field is found, skip this member
            if (property == null && field == null)
            {
                continue;
            }

            if (initialValue != null)
            {
                AddReadonlyControl(visualMember, readonlyMembers, node, field, property, initialValue, updateControls, spinBoxes);
            }
            else
            {
                _ = TryAddReadonlyControlAsync(visualMember, readonlyMembers, node, field, property, updateControls, spinBoxes);
            }
        }
    }

    /// <summary>
    /// Creates a canvas layer for a Node with a specified name and instance ID.
    /// </summary>
    private static CanvasLayer CreateCanvasLayer(string name, ulong instanceId)
    {
        CanvasLayer canvasLayer = new();
        canvasLayer.FollowViewportEnabled = true;
        canvasLayer.Name = $"Visualizing {name} {instanceId}";
        return canvasLayer;
    }

    /// <summary>
    /// Asynchronously tries to add a visual control for a Node member.
    /// </summary>
    private static async Task TryAddReadonlyControlAsync(string visualMember, VBoxContainer readonlyMembers, Node node, FieldInfo field, PropertyInfo property, List<Action> updateControls, List<VisualSpinBox> spinBoxes)
    {
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        int elapsedSeconds = 0;

        while (!token.IsCancellationRequested)
        {
            object value = null;

            if (field != null)
            {
                value = field.GetValue(node);
            }
            else if (property != null)
            {
                value = property.GetValue(node);
            }

            if (value != null)
            {
                AddReadonlyControl(visualMember, readonlyMembers, node, field, property, value, updateControls, spinBoxes);
                break;
            }

            try
            {
                await Task.Delay(1000, token);
                elapsedSeconds++;

                const int MAX_WAIT_TIME = 3;

                if (elapsedSeconds == MAX_WAIT_TIME)
                {
                    string memberName = string.Empty;

                    if (field != null)
                    {
                        memberName = field.Name;
                    }
                    else if (property != null)
                    {
                        memberName = property.Name;
                    }

                    GD.PrintRich($"[color=orange][Visualize] Tracking '{node.Name}' to see if '{memberName}' value changes[/color]");
                }
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, exit the loop
                break;
            }
        }
    }

    /// <summary>
    /// Adds a visual control to the UI for a Node member.
    /// </summary>
    private static void AddReadonlyControl(string visualMember, VBoxContainer readonlyMembers, Node node, FieldInfo field, PropertyInfo property, object initialValue, List<Action> updateControls, List<VisualSpinBox> spinBoxes)
    {
        Type memberType = property != null ? property.PropertyType : field.FieldType;

        MemberInfo member = property != null ? property : field;

        VisualControlContext context = new(spinBoxes, initialValue, v =>
        {
            // Do nothing
        });

        VisualControlInfo visualControlInfo = VisualControlTypes.CreateControlForType(memberType, member, context);

        visualControlInfo.VisualControl.SetEditable(false);

        updateControls.Add(() =>
        {
            object newValue = property != null
                ? property.GetValue(property.GetGetMethod(true).IsStatic ? null : node)
                : field.GetValue(field.IsStatic ? null : node);

            visualControlInfo.VisualControl.SetValue(newValue);
        });

        HBoxContainer hbox = new();
        hbox.Name = visualMember;

        hbox.AddChild(new Label { Text = visualMember });
        hbox.AddChild(visualControlInfo.VisualControl.Control);

        readonlyMembers.AddChild(hbox);
    }

    /// <summary>
    /// Adds member information elements to a VBoxContainer.
    /// </summary>
    private static void AddMutableControls(VBoxContainer vbox, IEnumerable<MemberInfo> members, Node node, List<VisualSpinBox> spinBoxes)
    {
        foreach (MemberInfo member in members)
        {
            Control element = CreateMemberInfoElement(member, node, spinBoxes);

            if (element != null)
                vbox.AddChild(element);
        }
    }

    /// <summary>
    /// Creates a member information element for a specified Node member.
    /// </summary>
    private static Control CreateMemberInfoElement(MemberInfo member, Node node, List<VisualSpinBox> spinBoxes)
    {
        Type type = VisualHandler.GetMemberType(member);

        object initialValue = VisualHandler.GetMemberValue(member, node);

        if (initialValue == null)
        {
            PrintUtils.Warning($"[Visualize] '{member.Name}' value in '{node.Name}' is null");
            return null;
        }

        VisualControlInfo element = VisualControlTypes.CreateControlForType(type, member, new VisualControlContext(spinBoxes, initialValue, v => 
        {
            VisualHandler.SetMemberValue(member, node, v);
        }));

        Control container = null;
        Label label = null;

        if (element.VisualControl is ClassControl)
        {
            container = new VBoxContainer();
            label = new()
            {
                LabelSettings = new LabelSettings()
                {
                    FontSize = 18,
                    OutlineSize = 6,
                    OutlineColor = Colors.Black,
                }
            };
        }
        else
        {
            container = new HBoxContainer();
            label = new();
        }

        label.Text = member.Name.ToPascalCase().AddSpaceBeforeEachCapital();
        label.HorizontalAlignment = HorizontalAlignment.Center;
        container.Name = member.Name;

        if (element.VisualControl != null)
        {
            container.AddChild(label);
            container.AddChild(element.VisualControl.Control);
        }

        return container;
    }
}
