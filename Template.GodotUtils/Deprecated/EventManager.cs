using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotUtils.Deprecated;

/// <summary>
/// Legacy event manager for dispatching events by enum key.
/// </summary>
/// <typeparam name="TEvent">Enum key type used to group and dispatch listeners.</typeparam>
// This class was created to attempt to simplify the process of creating C# events for gamedev.
// 
// ########### Example #1 ###########
// 
// Events.Generic.AddListener(EventGeneric.OnKeyboardInput, (args) => 
// {
//     GD.Print(args[0]);
//     GD.Print(args[1]);
//     GD.Print(args[2]);
// }, "someId");
// 
// Events.Generic.RemoveListeners(EventGeneric.OnKeyboardInput, "someId");
// 
// // Listener is never called because it was removed
// Events.Generic.Notify(EventGeneric.OnKeyboardInput, 1, 2, 3);
// 
// ########### Example #2 ###########
// Events.Player.AddListener<PlayerSpawnArgs>(EventPlayer.OnPlayerSpawn, (args) => 
// {
//     GD.Print(args.Name);
//     GD.Print(args.Location);
//     GD.Print(args.Player);
// });
// 
// Events.Player.Notify(EventPlayer.OnPlayerSpawn, new PlayerSpawnArgs(name, location, player));
// <typeparam name="TEvent">The event type enum to be used. For example 'EventPlayer' enum.</typeparam>
public class EventManager<TEvent> where TEvent : notnull
{
    private readonly Dictionary<TEvent, List<Listener>> _eventListeners = [];

    /// <summary>
    /// Adds a listener that receives raw argument arrays.
    /// </summary>
    /// <param name="eventType">Event key to subscribe to.</param>
    /// <param name="action">Callback invoked with all notify arguments.</param>
    /// <param name="id">Optional identifier used for grouped removal.</param>
    public void AddListener(TEvent eventType, Action<object[]> action, string id = "")
    {
        AddListenerInternal(eventType, action, id);
    }

    /// <summary>
    /// Adds a typed listener that receives the first argument as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Expected type of the first notify argument.</typeparam>
    /// <param name="eventType">Event key to subscribe to.</param>
    /// <param name="action">Typed callback invoked with the first notify argument.</param>
    /// <param name="id">Optional identifier used for grouped removal.</param>
    public void AddListener<T>(TEvent eventType, Action<T> action, string id = "")
    {
        AddListenerInternal(eventType, WrapAction(action), id);
    }

    /// <summary>
    /// Removes all listeners of type <paramref name="eventType"/> with the provided id.
    /// </summary>
    /// <param name="eventType">Event key whose listeners should be filtered.</param>
    /// <param name="id">Identifier used to match listener entries for removal.</param>
    public void RemoveListeners(TEvent eventType, string id = "")
    {
        // Removing from an unknown event type is treated as a usage error. 
        if (!_eventListeners.TryGetValue(eventType, out List<Listener>? listeners))
            throw new InvalidOperationException($"Tried to remove listener of event type '{eventType}' from an event type that has not even been defined yet");

        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            // Remove all listener entries that match the target id.
            if (listeners[i].Id == id)
                listeners.RemoveAt(i);
        }
    }

    /// <summary>
    /// Removes all listeners from all event types.
    /// </summary>
    public void RemoveAllListeners()
    {
        _eventListeners.Clear();
    }

    /// <summary>
    /// Notifies all listeners for the provided event type.
    /// </summary>
    /// <param name="eventType">Event key to dispatch.</param>
    /// <param name="args">Arguments passed through to each listener callback.</param>
    public void Notify(TEvent eventType, params object[] args)
    {
        // Unknown event types simply have no listeners to notify.
        if (!_eventListeners.TryGetValue(eventType, out List<Listener>? value))
            return;

        // Iterate snapshot to avoid collection-modified issues during callbacks.
        foreach (Listener listener in value.ToList()) // if ToList() is not here then issue #137 will occur
            ((Action<object[]>)listener.Action)(args);
    }

    /// <summary>
    /// Registers a raw listener for the given event type, creating the listener list when needed.
    /// </summary>
    /// <param name="eventType">Event key used to group listeners.</param>
    /// <param name="action">Listener callback that receives the notify argument array.</param>
    /// <param name="id">Optional listener identifier used by removal APIs.</param>
    private void AddListenerInternal(TEvent eventType, Action<object[]> action, string id)
    {
        // Lazily create listener bucket for first subscription of an event type.
        if (!_eventListeners.TryGetValue(eventType, out List<Listener>? listeners))
        {
            listeners = [];
            _eventListeners.Add(eventType, listeners);
        }

        listeners.Add(new Listener(action, id));
    }

    /// <summary>
    /// Wraps a typed listener so it can be invoked by the raw object-array dispatch pipeline.
    /// </summary>
    /// <typeparam name="T">Expected argument type at index zero.</typeparam>
    /// <param name="action">Typed listener callback.</param>
    /// <returns>Adapter callback compatible with raw listener storage.</returns>
    private static Action<object[]> WrapAction<T>(Action<T> action)
    {
        return args =>
        {
            // Typed listeners consume only the first argument when available.
            if (args == null || args.Length == 0)
                return;

            action((T)args[0]);
        };
    }
}

/// <summary>
/// Stores a listener delegate and identifier.
/// </summary>
/// <param name="action">Callback delegate to invoke during event dispatch.</param>
/// <param name="id">Optional identifier used for listener removal filters.</param>
public class Listener(dynamic action, string id)
{
    /// <summary>
    /// Gets or sets the listener action.
    /// </summary>
    public dynamic Action { get; set; } = action;

    /// <summary>
    /// Gets or sets the listener identifier.
    /// </summary>
    public string Id { get; set; } = id;
}
