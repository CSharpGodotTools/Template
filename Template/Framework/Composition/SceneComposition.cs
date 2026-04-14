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
    /// <param name="services">Runtime services used for configuration.</param>
    /// <returns>The configured node instance.</returns>
    public static Node InstantiateAndConfigure(PackedScene scene, GameServices services)
    {
        return InstantiateAndConfigure<Node>(scene, services);
    }

    /// <summary>
    /// Instantiates a scene of type <typeparamref name="T"/> and configures dependencies.
    /// </summary>
    /// <typeparam name="T">Concrete node type expected from the packed scene.</typeparam>
    /// <param name="scene">Packed scene to instantiate.</param>
    /// <param name="services">Runtime services used for configuration.</param>
    /// <returns>The configured node instance.</returns>
    public static T InstantiateAndConfigure<T>(PackedScene scene, GameServices services) where T : Node
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(services);

        T node = scene.Instantiate<T>();
        ConfigureNode(node, services);
        return node;
    }

    /// <summary>
    /// Configures the provided root node.
    /// </summary>
    /// <param name="root">Root node to configure.</param>
    /// <param name="services">Runtime services used for configuration.</param>
    public static void ConfigureNodeTree(Node root, GameServices services)
    {
        ConfigureNode(root, services);
    }

    /// <summary>
    /// Configures a single node when it implements <see cref="ISceneDependencyReceiver"/>.
    /// </summary>
    /// <param name="node">Node candidate to configure.</param>
    /// <param name="services">Runtime services used for configuration.</param>
    public static void ConfigureNode(Node node, GameServices services)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(services);

        // Configure only nodes that explicitly accept scene dependencies.
        if (node is ISceneDependencyReceiver receiver)
            receiver.Configure(services);
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
        if (!Game.TryGetServices(out GameServices services))
            return;

        receiver.Configure(services);
    }
}
