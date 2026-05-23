using Godot;
using System;

namespace __TEMPLATE__;

/// <summary>
/// Composes scene instances by applying runtime service configuration.
/// </summary>
public static class SceneComposition
{
    /// <summary>
    /// Instantiates a scene as <see cref="Node"/> and configures dependencies.
    /// </summary>
    /// <param name="scene">Packed scene to instantiate.</param>
    /// <param name="framework">Runtime services used for configuration.</param>
    /// <returns>The configured node instance.</returns>
    public static Node InstantiateAndConfigure(PackedScene scene, AutoloadsFramework framework)
    {
        return InstantiateAndConfigure<Node>(scene, framework);
    }

    /// <summary>
    /// Instantiates a scene of type <typeparamref name="T"/> and configures dependencies.
    /// </summary>
    /// <typeparam name="T">Concrete node type expected from the packed scene.</typeparam>
    /// <param name="scene">Packed scene to instantiate.</param>
    /// <param name="framework">Runtime services used for configuration.</param>
    /// <returns>The configured node instance.</returns>
    public static T InstantiateAndConfigure<T>(PackedScene scene, AutoloadsFramework framework) where T : Node
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(framework);

        T node = scene.Instantiate<T>();
        ConfigureNode(node, framework);
        return node;
    }

    /// <summary>
    /// Configures the provided root node.
    /// </summary>
    /// <param name="root">Root node to configure.</param>
    /// <param name="framework">Runtime services used for configuration.</param>
    public static void ConfigureNodeTree(Node root, AutoloadsFramework framework)
    {
        ConfigureNode(root, framework);
    }

    /// <summary>
    /// Configures a single node when it implements <see cref="ISceneDependencyReceiver"/>.
    /// </summary>
    /// <param name="node">Node candidate to configure.</param>
    /// <param name="framework">Runtime services used for configuration.</param>
    public static void ConfigureNode(Node node, AutoloadsFramework framework)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(framework);

        // Configure only nodes that explicitly accept scene dependencies.
        if (node is ISceneDependencyReceiver receiver)
            receiver.Configure(framework);
    }

    /// <summary>
    /// Configures a node using services from <see cref="Game"/> when available.
    /// </summary>
    /// <param name="node">Node candidate to configure.</param>
    public static void ConfigureNodeFromGame(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        // Exit early when the node does not opt into dependency injection.
        if (node is not ISceneDependencyReceiver receiver)
            return;

        // Exit early when game-level services are not yet available.
        if (!Game.TryGetFramework(out AutoloadsFramework framework))
            return;

        receiver.Configure(framework);
    }
}
