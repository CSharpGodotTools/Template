using Godot;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Utility class for handling cursor-related operations in a 2D Godot scene.
/// </summary>
public static class CursorUtils
{
    /// <summary>
    /// Returns area nodes under the mouse cursor.
    /// </summary>
    /// <param name="node">Node providing world and cursor context.</param>
    /// <param name="maxResults">Maximum number of hits to return.</param>
    /// <returns>Matched area nodes.</returns>
    public static List<Area2D> GetAreasUnderCursor(Node2D node, int maxResults = 1)
    {
        return GetPhysicsNodesAtPosition<Area2D>(node, node.GetGlobalMousePosition(), true, false, false, maxResults);
    }

    /// <summary>
    /// Returns physics body nodes under the mouse cursor.
    /// </summary>
    /// <param name="node">Node providing world and cursor context.</param>
    /// <param name="maxResults">Maximum number of hits to return.</param>
    /// <returns>Matched body nodes.</returns>
    public static List<PhysicsBody2D> GetBodiesUnderCursor(Node2D node, int maxResults = 1)
    {
        return GetPhysicsNodesAtPosition<PhysicsBody2D>(node, node.GetGlobalMousePosition(), false, true, false, maxResults);
    }

    /// <summary>
    /// Returns area nodes under the provided node position.
    /// </summary>
    /// <param name="node">Node providing world and position context.</param>
    /// <param name="maxResults">Maximum number of hits to return.</param>
    /// <returns>Matched area nodes.</returns>
    public static List<Area2D> GetAreasUnder(Node2D node, int maxResults = 1)
    {
        return GetPhysicsNodesAtPosition<Area2D>(node, node.GlobalPosition, true, false, true, maxResults);
    }

    /// <summary>
    /// Returns physics body nodes under the provided node position.
    /// </summary>
    /// <param name="node">Node providing world and position context.</param>
    /// <param name="maxResults">Maximum number of hits to return.</param>
    /// <returns>Matched body nodes.</returns>
    public static List<PhysicsBody2D> GetBodiesUnder(Node2D node, int maxResults = 1)
    {
        return GetPhysicsNodesAtPosition<PhysicsBody2D>(node, node.GlobalPosition, false, true, true, maxResults);
    }

    /// <summary>
    /// Executes a point query and returns colliders of the requested node type.
    /// </summary>
    /// <typeparam name="TNode">Expected collider node type.</typeparam>
    /// <param name="node">Node used to resolve world/space state.</param>
    /// <param name="position">World position to query.</param>
    /// <param name="collideWithAreas">Include <see cref="Area2D"/> colliders.</param>
    /// <param name="collideWithBodies">Include <see cref="PhysicsBody2D"/> colliders.</param>
    /// <param name="excludeSelf">Exclude the source node hierarchy from hits.</param>
    /// <param name="maxResults">Maximum hit count.</param>
    /// <returns>Matching collider nodes.</returns>
    private static List<TNode> GetPhysicsNodesAtPosition<TNode>(Node2D node, Vector2 position, bool collideWithAreas, bool collideWithBodies, bool excludeSelf = false, int maxResults = 1)
        where TNode : Node
    {
        // Configure point-query parameters.
        PhysicsPointQueryParameters2D queryParams = new()
        {
            Position = position,
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies
        };

        // Optionally exclude the source node and its collision children.
        if (excludeSelf)
        {
            List<Rid> rids = [];

            foreach (Node child in node.GetChildren<Node>())
            {
                // Collect RIDs for collision objects to exclude from results.
                if (child is CollisionObject2D collision)
                {
                    rids.Add(collision.GetRid());
                }
            }

            queryParams.Exclude = [.. rids];
        }

        // Execute query in the current 2D physics space.
        PhysicsDirectSpaceState2D spaceState = PhysicsServer2D.SpaceGetDirectState(node.GetWorld2D().GetSpace());

        Godot.Collections.Array<Godot.Collections.Dictionary> results = spaceState.IntersectPoint(queryParams, maxResults);

        int resultCount = results.Count;
        List<TNode> nodes = new(resultCount);

        foreach (Godot.Collections.Dictionary result in results)
        {
            // Ensure collider payload exists before casting.
            if (result != null && result.ContainsKey("collider"))
            {
                Node collider = result["collider"].As<Node>();

                // Keep only colliders matching requested type.
                if (collider is TNode typed)
                    nodes.Add(typed);
            }
        }

        return nodes;
    }
}
