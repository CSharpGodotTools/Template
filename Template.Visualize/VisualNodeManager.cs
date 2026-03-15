#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class VisualNodeManager
{
    private static readonly Type[] _positionNodeTypes = [typeof(Node2D), typeof(Control), typeof(Node3D)];

    private readonly Dictionary<ulong, VisualNodeInfo> _nodeTrackers = [];
    private readonly Visual2DNodeRegistrar _nodeRegistrar2D = new();
    private readonly Visual3DNodeRegistrar _nodeRegistrar3D = new();

    public void Register(Node node, object visualizedObject)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(visualizedObject);

        VisualData? visualData = VisualizeAttributeHandler.RetrieveData(visualizedObject, node);
        if (visualData == null)
        {
            void OnEmptyVisualDataTreeExited()
            {
                RemoveVisualNodesForAnchor(node);
                node.TreeExited -= OnEmptyVisualDataTreeExited;
            }

            node.TreeExited += OnEmptyVisualDataTreeExited;
            return;
        }

        (Control visualPanel, IReadOnlyList<Action> actions) = VisualUI.CreateVisualPanel(visualData);
        Node positionalNode = VisualNodeLocator.GetClosestParentOfType(node, _positionNodeTypes) ?? node;

        VisualNodeInfo nodeInfo = positionalNode is Node3D node3D
            ? _nodeRegistrar3D.Register(new Visual3DRegistrationContext(node, node3D, visualPanel, actions, _nodeTrackers.Values))
            : _nodeRegistrar2D.Register(new Visual2DRegistrationContext(node, positionalNode, visualPanel, actions, _nodeTrackers.Values));

        _nodeTrackers.Add(visualPanel.GetInstanceId(), nodeInfo);

        void OnTrackedNodeTreeExited()
        {
            RemoveVisualNodesForAnchor(node);
            node.TreeExited -= OnTrackedNodeTreeExited;
        }

        node.TreeExited += OnTrackedNodeTreeExited;
    }

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

    private void RemoveVisualNodesForAnchor(Node anchorNode)
    {
        List<ulong> trackersToRemove = [];

        foreach (KeyValuePair<ulong, VisualNodeInfo> tracker in _nodeTrackers)
        {
            if (tracker.Value.AnchorNode != anchorNode)
            {
                continue;
            }

            tracker.Value.VisualRoot.QueueFree();
            trackersToRemove.Add(tracker.Key);
        }

        foreach (ulong id in trackersToRemove)
        {
            _nodeTrackers.Remove(id);
        }

        VisualizeAutoload.Instance?.UnregisterNode(anchorNode);
    }
}
#endif
