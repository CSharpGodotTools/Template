using Godot;
using GodotUtils;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Template;

/// <summary>
/// Services have a scene lifetime meaning they will be destroyed when the scene changes. Services
/// aid as an alternative to using the static keyword everywhere.
/// </summary>
public partial class Services : Node
{
    /// <summary>
    /// Dictionary to store registered services, keyed by their type.
    /// </summary>
    private static Dictionary<Type, Service> _services = [];

    public override void _EnterTree()
    {
        GetTree().NodeAdded += AttemptToRegisterService;
    }

    /// <summary>
    /// Retrieves a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>The instance of the service.</returns>
    public static T Get<T>()
    {
        if (!_services.ContainsKey(typeof(T)))
        {
            throw new Exception($"Unable to obtain service '{typeof(T)}'");
        }

        return (T)_services[typeof(T)].Instance;
    }

    private void AttemptToRegisterService(Node node)
    {
        if (_services.ContainsKey(node.GetType()))
        {
            throw new Exception($"There can only be one service of type '{node.GetType().Name}'");
        }

        ServiceAttribute serviceAttribute = node.GetType().GetCustomAttribute<ServiceAttribute>();

        if (serviceAttribute != null)
        {
            //GD.Print($"Registering service: {node.GetType().Name}");
            AddService(node);
            return;
        }
    }

    /// <summary>
    /// Adds a service to the service provider.
    /// </summary>
    private void AddService(Node node)
    {
        Service service = new()
        {
            Instance = node
        };

        _services.Add(node.GetType(), service);

        RemoveServiceOnSceneChanged(service);
    }

    /// <summary>
    /// Removes a service when the scene changes.
    /// </summary>
    private void RemoveServiceOnSceneChanged(Service service)
    {
        // The scene has changed, remove all services
        SceneManager.PreSceneChanged += Cleanup;

        void Cleanup(string scene)
        {
            // Stop listening to PreSceneChanged
            SceneManager.PreSceneChanged -= Cleanup;

            // Remove the service
            bool success = _services.Remove(service.Instance.GetType());

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
