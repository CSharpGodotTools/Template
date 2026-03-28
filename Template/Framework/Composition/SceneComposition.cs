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
        ConfigureNodeTree(node, services);
        return node;
    }

    public static void ConfigureNodeTree(Node root, GameServices services)
    {
        if (root is ISceneDependencyReceiver receiver)
        {
            receiver.Configure(services);
        }

        foreach (Node child in root.GetChildren())
        {
            ConfigureNodeTree(child, services);
        }
    }
}
