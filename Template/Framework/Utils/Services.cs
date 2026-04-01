using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__;

/// <summary>
/// Services have a scene lifetime meaning they will be destroyed when the scene changes. Services
/// aid as an alternative to using the static keyword everywhere.
/// </summary>
/// <param name="autoloads">Autoload container used to subscribe to scene lifecycle events.</param>
public class Services(AutoloadsFramework autoloads)
{
    /// <summary>
    /// Dictionary to store registered services, keyed by their type.
    /// </summary>
    private readonly Dictionary<Type, Node> _services = [];
    private readonly SceneManager _sceneManager = autoloads.SceneManager;
    private bool _isCleanupSubscribed;

    // API
    /// <summary>
    /// Retrieves a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>The instance of the service.</returns>
    public T Get<T>()
    {
        Type serviceType = typeof(T);

        // Throw when the requested service type was not registered for this scene.
        if (!_services.TryGetValue(serviceType, out Node? service))
            throw new InvalidOperationException($"Unable to obtain service '{serviceType.Name}'.");

        return (T)(object)service!;
    }

    /// <summary>
    /// Registers the given <see cref="Node"/> as a singleton-style service for the current scene.
    /// Only one service of a particular type may be registered at a time, and it will be
    /// automatically unregistered when the scene changes.
    /// </summary>
    /// <param name="node">The node to register as a service.</param>
    /// <exception cref="Exception">
    /// Thrown if a service of the same <see cref="Type"/> has already been registered.
    /// </exception>
    public void Register(Node node)
    {
        Type serviceType = node.GetType();

        // Enforce one service instance per concrete type.
        if (_services.ContainsKey(serviceType))
            throw new InvalidOperationException($"There can only be one service of type '{serviceType.Name}'.");

        AddService(node);
    }

    // Private Methods
    /// <summary>
    /// Adds a service to the service provider.
    /// </summary>
    /// <param name="node">Service node instance to register.</param>
    private void AddService(Node node)
    {
        _services.Add(node.GetType(), node);
        EnsureCleanupSubscription();
    }

    /// <summary>
    /// Subscribes to scene cleanup exactly once.
    /// </summary>
    private void EnsureCleanupSubscription()
    {
        // Avoid duplicate scene-change subscriptions.
        if (_isCleanupSubscribed)
        {
            return;
        }

        _sceneManager.PreSceneChanged += CleanupOnSceneChanged;
        _isCleanupSubscribed = true;
    }

    /// <summary>
    /// Removes all services when the active scene changes.
    /// </summary>
    private void CleanupOnSceneChanged()
    {
        _services.Clear();
        _sceneManager.PreSceneChanged -= CleanupOnSceneChanged;
        _isCleanupSubscribed = false;
    }

    /// <summary>
    /// Returns a formatted string containing all registered services.
    /// </summary>
    /// <returns>Formatted text representation of registered services.</returns>
    public override string ToString()
    {
        return _services.ToFormattedString()!;
    }
}
