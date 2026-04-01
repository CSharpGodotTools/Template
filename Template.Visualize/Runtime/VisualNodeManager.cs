#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Tracks runtime visualization nodes and keeps them synchronized with their anchor nodes.
/// </summary>
internal sealed class VisualNodeManager
{
    private static readonly Type[] _positionNodeTypes = [typeof(Node2D), typeof(Control), typeof(Node3D)];

    private readonly Dictionary<ulong, VisualNodeInfo> _nodeTrackers = [];
    private readonly Visual2DNodeRegistrar _nodeRegistrar2D = new();
    private readonly Visual3DNodeRegistrar _nodeRegistrar3D = new();

    /// <summary>
    /// Registers a scene node for visualization and creates the corresponding visual representation.
    /// </summary>
    /// <param name="node">Anchor node to visualize.</param>
    /// <param name="visualizedObject">Object inspected for visualize attributes.</param>
    public void Register(Node node, object visualizedObject)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(visualizedObject);

        // Build visualization metadata from attributes on the target object.
        VisualData? visualData = VisualizeAttributeHandler.RetrieveData(visualizedObject, node);

        // Skip panel creation when no visualize metadata is available.
        if (visualData == null)
        {
            // Even without visual members, ensure stale visuals are removed when the anchor exits.
            void OnEmptyVisualDataTreeExited()
            {
                RemoveVisualNodesForAnchor(node);
                node.TreeExited -= OnEmptyVisualDataTreeExited;
            }

            node.TreeExited += OnEmptyVisualDataTreeExited;
            return;
        }

        // Create UI controls and choose the nearest node that provides positional context.
        (Control visualPanel, IReadOnlyList<Action> actions) = VisualUI.CreateVisualPanel(visualData);
        Node positionalNode = VisualNodeLocator.GetClosestParentOfType(node, _positionNodeTypes) ?? node;

        // Route registration through 2D or 3D pipelines so follow behavior matches anchor space.
        VisualNodeInfo nodeInfo = positionalNode is Node3D node3D
            ? _nodeRegistrar3D.Register(new Visual3DRegistrationContext(node, node3D, visualPanel, actions, _nodeTrackers.Values))
            : _nodeRegistrar2D.Register(new Visual2DRegistrationContext(node, positionalNode, visualPanel, actions, _nodeTrackers.Values));

        _nodeTrackers.Add(visualPanel.GetInstanceId(), nodeInfo);

        // Tear down associated visual nodes automatically when the tracked anchor leaves the tree.
        void OnTrackedNodeTreeExited()
        {
            RemoveVisualNodesForAnchor(node);
            node.TreeExited -= OnTrackedNodeTreeExited;
        }

        node.TreeExited += OnTrackedNodeTreeExited;
    }

    /// <summary>
    /// Updates position and displayed values for all tracked visualization nodes.
    /// </summary>
    public void Update()
    {
        foreach (VisualNodeInfo info in _nodeTrackers.Values)
        {
            info.UpdatePosition();

            foreach (Action action in info.Actions)
            {
                action();
            }
        }
    }

    /// <summary>
    /// Removes all visualization nodes associated with an anchor node and unregisters it globally.
    /// </summary>
    /// <param name="anchorNode">Anchor node whose visualization entries should be removed.</param>
    private void RemoveVisualNodesForAnchor(Node anchorNode)
    {
        List<ulong> trackersToRemove = [];

        foreach (KeyValuePair<ulong, VisualNodeInfo> tracker in _nodeTrackers)
        {
            // Only collect trackers that belong to the anchor currently being removed.
            if (tracker.Value.AnchorNode != anchorNode)
            {
                continue;
            }

            tracker.Value.VisualRoot.QueueFree();
            trackersToRemove.Add(tracker.Key);
        }

        // Remove by key after iteration to avoid mutating the dictionary during enumeration.
        foreach (ulong id in trackersToRemove)
        {
            _nodeTrackers.Remove(id);
        }

        // Keep autoload registry aligned with local tracker cleanup.
        VisualizeAutoload.Instance?.UnregisterNode(anchorNode);
    }
}
#endif
