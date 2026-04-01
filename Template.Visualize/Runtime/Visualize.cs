#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Debug visualization entry point for registering nodes and writing in-world log labels.
/// </summary>
public static class Visualize
{
    private const double DefaultFadeTimeSeconds = 5;
    private const int MaxLabelsVisible = 5;
    private static readonly VisualNodeManager _visualNodeManager = new();

    /// <summary>
    /// Registers a node as both visualization source and anchor.
    /// </summary>
    /// <param name="node">Node to register.</param>
    public static void Register(Node node)
    {
        Register(node, node);
    }

    /// <summary>
    /// Registers a visualization source object with an anchor node.
    /// </summary>
    /// <param name="visualizedObject">Object inspected for <see cref="VisualizeAttribute"/> members.</param>
    /// <param name="node">Anchor node for positioning generated visuals.</param>
    public static void Register(object visualizedObject, Node node)
    {
        ArgumentNullException.ThrowIfNull(visualizedObject);
        ArgumentNullException.ThrowIfNull(node);

        // Registration requires the debug autoload to be active.
        if (VisualizeAutoload.Instance == null)
        {
            PrintUtils.Warning("[Visualize] VisualizeAutoload is not initialized.");
            return;
        }

        _visualNodeManager.Register(node, visualizedObject);
    }

    /// <summary>
    /// Updates all tracked visualization nodes for the current frame.
    /// </summary>
    public static void Update()
    {
        _visualNodeManager.Update();
    }

    /// <summary>
    /// Writes a transient log message near the provided node.
    /// </summary>
    /// <param name="message">Message payload to display.</param>
    /// <param name="node">Node whose log container receives the message.</param>
    /// <param name="fadeTime">Fade duration in seconds before the label is freed.</param>
    public static void Log(object message, Node node, double fadeTime = DefaultFadeTimeSeconds)
    {
        ArgumentNullException.ThrowIfNull(node);

        VBoxContainer? vbox = GetOrCreateVBoxContainer(node);

        // Skip label creation when no container can be resolved for this node type/state.
        if (vbox != null)
            AddLabel(vbox, message, fadeTime);
    }

    /// <summary>
    /// Resolves an existing log container for a node or creates a fallback container when valid.
    /// </summary>
    /// <param name="node">Node requesting log output.</param>
    /// <returns>Log container for the node, or <see langword="null"/> when logging is unsupported.</returns>
    private static VBoxContainer? GetOrCreateVBoxContainer(Node node)
    {
        VisualizeAutoload? autoload = VisualizeAutoload.Instance;

        // No autoload means logging cannot be attached anywhere.
        if (autoload == null)
            return null;

        // Prefer attribute-driven containers when one has already been registered.
        if (autoload.TryGetLogContainer(node, out VBoxContainer? vbox))
            return vbox;

        // Fallback containers are only supported for nodes with a drawable/control hierarchy.
        if (node is not Control and not Node2D)
            return null;

        return autoload.GetOrCreateNonAttributeLogContainer(node, () =>
        {
            VBoxContainer container = new() { Scale = Vector2.One * VisualUiLayout.LogScaleFactor };
            node.AddChild(container);
            return container;
        });
    }

    /// <summary>
    /// Creates a log label, inserts it at the top of the container, and schedules fade-out cleanup.
    /// </summary>
    /// <param name="vbox">Container receiving the label.</param>
    /// <param name="message">Message payload to display.</param>
    /// <param name="fadeTime">Fade duration in seconds.</param>
    private static void AddLabel(VBoxContainer vbox, object message, double fadeTime)
    {
        Label label = new() { Text = message?.ToString() ?? string.Empty };

        vbox.AddChild(label);
        vbox.MoveChild(label, 0);

        // Enforce a bounded label count so repeated logs do not grow the panel indefinitely.
        if (vbox.GetChildCount() > MaxLabelsVisible)
            vbox.RemoveChild(vbox.GetChild(vbox.GetChildCount() - 1));

        Tweens.Animate(label)
            .ColorRecursive(Colors.Transparent, fadeTime)
            .Then(label.QueueFree);
    }
}
#endif
