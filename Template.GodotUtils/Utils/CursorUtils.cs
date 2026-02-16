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
    public static List<Area2D> GetAreasUnderCursor(Node2D node, int maxResults = 1)
    {
        return GetPhysicsNodesAtPosition<Area2D>(node, node.GetGlobalMousePosition(), true, false, false, maxResults);
    }
    
    /// <summary>
    /// Returns physics body nodes under the mouse cursor.
    /// </summary>
    public static List<PhysicsBody2D> GetBodiesUnderCursor(Node2D node, int maxResults = 1)
    {
        return GetPhysicsNodesAtPosition<PhysicsBody2D>(node, node.GetGlobalMousePosition(), false, true, false, maxResults);
    }
    
    /// <summary>
    /// Returns area nodes under the provided node position.
    /// </summary>
    public static List<Area2D> GetAreasUnder(Node2D node, int maxResults = 1)
    {
        return GetPhysicsNodesAtPosition<Area2D>(node, node.GlobalPosition, true, false, true, maxResults);
    }
    
    /// <summary>
    /// Returns physics body nodes under the provided node position.
    /// </summary>
    public static List<PhysicsBody2D> GetBodiesUnder(Node2D node, int maxResults = 1)
    {
        return GetPhysicsNodesAtPosition<PhysicsBody2D>(node, node.GlobalPosition, false, true, true, maxResults);
    }
    
    private static List<TNode> GetPhysicsNodesAtPosition<TNode>(Node2D node, Vector2 position, bool collideWithAreas, bool collideWithBodies, bool excludeSelf = false, int maxResults = 1)
        where TNode : Node
    {
        // Create a shape query parameters object
        PhysicsPointQueryParameters2D queryParams = new()
        {
            Position = position,
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies
        };

        // Exclude the node itself and its children from the query
        if (excludeSelf)
        {
            List<Rid> rids = [];

            foreach (Node child in node.GetChildren<Node>())
            {
                if (child is CollisionObject2D collision)
                {
                    rids.Add(collision.GetRid());
                }
            }

            queryParams.Exclude = [.. rids];
        }

        // Perform the query
        PhysicsDirectSpaceState2D spaceState = PhysicsServer2D.SpaceGetDirectState(node.GetWorld2D().GetSpace());
        
        Godot.Collections.Array<Godot.Collections.Dictionary> results = spaceState.IntersectPoint(queryParams, maxResults);

        int resultCount = results.Count;
        List<TNode> nodes = new(resultCount);

        foreach (Godot.Collections.Dictionary result in results)
        {
            if (result != null && result.ContainsKey("collider"))
            {
                Node collider = result["collider"].As<Node>();
                if (collider is TNode typed)
                    nodes.Add(typed);
            }
        }

        return nodes;
    }
}
