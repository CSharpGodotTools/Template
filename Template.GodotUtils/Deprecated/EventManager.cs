using System.Collections.Generic;
using System.Linq;
using System;

namespace GodotUtils.Deprecated;

/// <summary>
/// Legacy event manager for dispatching events by enum key.
/// </summary>
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
public class EventManager<TEvent>
{
    private readonly Dictionary<TEvent, List<Listener>> _eventListeners = [];

    /// <summary>
    /// Adds a listener that receives raw argument arrays.
    /// </summary>
    public void AddListener(TEvent eventType, Action<object[]> action, string id = "")
    {
        AddListenerInternal(eventType, action, id);
    }

    /// <summary>
    /// Adds a typed listener that receives the first argument as <typeparamref name="T"/>.
    /// </summary>
    public void AddListener<T>(TEvent eventType, Action<T> action, string id = "")
    {
        AddListenerInternal(eventType, WrapAction(action), id);
    }

    /// <summary>
    /// Removes all listeners of type <paramref name="eventType"/> with the provided id.
    /// </summary>
    public void RemoveListeners(TEvent eventType, string id = "")
    {
        if (!_eventListeners.TryGetValue(eventType, out List<Listener> listeners))
            throw new InvalidOperationException($"Tried to remove listener of event type '{eventType}' from an event type that has not even been defined yet");

        for (int i = listeners.Count - 1; i >= 0; i--)
        {
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
    public void Notify(TEvent eventType, params object[] args)
    {
        if (!_eventListeners.TryGetValue(eventType, out List<Listener> value))
        {
            return;
        }

        foreach (Listener listener in value.ToList()) // if ToList() is not here then issue #137 will occur
        {
            ((Action<object[]>)listener.Action)(args);
        }
    }

    private void AddListenerInternal(TEvent eventType, Action<object[]> action, string id)
    {
        if (!_eventListeners.TryGetValue(eventType, out List<Listener> listeners))
        {
            listeners = [];
            _eventListeners.Add(eventType, listeners);
        }

        listeners.Add(new Listener(action, id));
    }

    private static Action<object[]> WrapAction<T>(Action<T> action)
    {
        return args =>
        {
            if (args == null || args.Length == 0)
                return;

            action((T)args[0]);
        };
    }
}

/// <summary>
/// Stores a listener delegate and identifier.
/// </summary>
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
