using Godot;
using System;

namespace __TEMPLATE__;

public static class SceneComposition
{
    public static Node InstantiateAndConfigure(PackedScene scene, GameServices services)
    {
        return InstantiateAndConfigure<Node>(scene, services);
    }

    public static T InstantiateAndConfigure<T>(PackedScene scene, GameServices services) where T : Node
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(services);

        T node = scene.Instantiate<T>();
        ConfigureNode(node, services);
        return node;
    }

    public static void ConfigureNodeTree(Node root, GameServices services)
    {
        ConfigureNode(root, services);
    }

    public static void ConfigureNode(Node node, GameServices services)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(services);

        if (node is ISceneDependencyReceiver receiver)
        {
            receiver.Configure(services);
        }
    }

    public static void ConfigureNodeFromGame(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node is not ISceneDependencyReceiver receiver)
        {
            return;
        }

        if (!Game.TryGetServices(out GameServices services))
        {
            return;
        }

        receiver.Configure(services);
    }
}
