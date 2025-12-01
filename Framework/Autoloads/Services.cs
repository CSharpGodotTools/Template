using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Services have a scene lifetime meaning they will be destroyed when the scene changes. Services
/// aid as an alternative to using the static keyword everywhere.
/// </summary>
public partial class Services : IDisposable
{
    /// <summary>
    /// Dictionary to store registered services, keyed by their type.
    /// </summary>
    private static Services _instance;
    private Dictionary<Type, Service> _services = [];
    private SceneManager _sceneManager;

    public Services(Autoloads autoloads)
    {
        if (_instance != null)
            throw new InvalidOperationException($"{nameof(Services)} was initialized already");

        _instance = this;
        _sceneManager = autoloads.SceneManager;
    }

    public void Dispose()
    {
        _instance = null;
    }

    /// <summary>
    /// Retrieves a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>The instance of the service.</returns>
    public static T Get<T>()
    {
        if (!_instance._services.ContainsKey(typeof(T)))
        {
            throw new Exception($"Unable to obtain service '{typeof(T)}'");
        }

        return (T)_instance._services[typeof(T)].Instance;
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
    public static void Register(Node node)
    {
        if (_instance._services.ContainsKey(node.GetType()))
        {
            throw new Exception($"There can only be one service of type '{node.GetType().Name}'");
        }

        //GD.Print($"Registering service: {node.GetType().Name}");
        AddService(node);
    }

    /// <summary>
    /// Adds a service to the service provider.
    /// </summary>
    private static void AddService(Node node)
    {
        Service service = new()
        {
            Instance = node
        };

        _instance._services.Add(node.GetType(), service);

        RemoveServiceOnSceneChanged(service);
    }

    /// <summary>
    /// Removes a service when the scene changes.
    /// </summary>
    private static void RemoveServiceOnSceneChanged(Service service)
    {
        // The scene has changed, remove all services
        _instance._sceneManager.PreSceneChanged += Cleanup;

        void Cleanup(string scene)
        {
            // Stop listening to PreSceneChanged
            _instance._sceneManager.PreSceneChanged -= Cleanup;

            // Remove the service
            bool success = _instance._services.Remove(service.Instance.GetType());

            if (!success)
            {
                throw new Exception($"Failed to remove the service '{service.Instance.GetType().Name}'");
            }
        }
    }

    /// <summary>
    /// A formatted string of the all the services.
    /// </summary>
    public override string ToString()
    {
        return _services.ToFormattedString();
    }

    /// <summary>
    /// A class representing a service instance
    /// </summary>
    public class Service
    {
        /// <summary>
        /// The instance of the service.
        /// </summary>
        public object Instance { get; set; }
    }
}
