#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// This logger shows all messages in game making it easier to debug
/// </summary>
public static class Visualize
{
    private const int MaxLabelsVisible = 5;
    private static readonly VisualNodeManager _visualNodeManager = new();

    public static void Register(Node node, params string[] readonlyMembers)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (VisualizeAutoload.Instance == null)
        {
            PrintUtils.Warning("[Visualize] VisualizeAutoload is not initialized.");
            return;
        }

        _visualNodeManager.Register(node, readonlyMembers);
    }

    public static void Update()
    {
        _visualNodeManager.Update();
    }

    public static void Log(object message, Node node, double fadeTime = 5)
    {
        ArgumentNullException.ThrowIfNull(node);

        VBoxContainer vbox = GetOrCreateVBoxContainer(node);

        if (vbox != null)
            AddLabel(vbox, message, fadeTime);
    }

    private static VBoxContainer GetOrCreateVBoxContainer(Node node)
    {
        VisualizeAutoload autoload = VisualizeAutoload.Instance;

        if (autoload == null)
            return null;

        if (autoload.TryGetLogContainer(node, out VBoxContainer vbox))
            return vbox;

        if (node is not Control and not Node2D)
            return null;

        return autoload.GetOrCreateNonAttributeLogContainer(node, () =>
        {
            VBoxContainer container = new() { Scale = Vector2.One * VisualUiLayout.LogScaleFactor };
            node.AddChild(container);
            return container;
        });
    }

    private static void AddLabel(VBoxContainer vbox, object message, double fadeTime)
    {
        Label label = new() { Text = message?.ToString() };

        vbox.AddChild(label);
        vbox.MoveChild(label, 0);

        if (vbox.GetChildCount() > MaxLabelsVisible)
            vbox.RemoveChild(vbox.GetChild(vbox.GetChildCount() - 1));

        Tweens.Animate(label)
            .ColorRecursive(Colors.Transparent, fadeTime)
            .Then(label.QueueFree);
    }
}
#endif
