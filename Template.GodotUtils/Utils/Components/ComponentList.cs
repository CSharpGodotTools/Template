using System.Collections.Generic;
using System.Linq;

namespace GodotUtils;

/// <summary>
/// Holds and manages a set of components.
/// </summary>
public class ComponentList
{
    private readonly HashSet<Component> _components = [];

    /// <summary>
    /// Add a component.
    /// </summary>
    public void Add(Component component)
    {
        _components.Add(component);
    }

    /// <summary>
    /// Add a list of components.
    /// </summary>
    public void Add(ComponentList components)
    {
        foreach (Component component in components._components)
        {
            _components.Add(component);
        }
    }

    /// <summary>
    /// Remove a component.
    /// </summary>
    public void Remove(Component component)
    {
        _components.Remove(component);
    }

    /// <summary>
    /// Remove all components.
    /// </summary>
    public void RemoveAll()
    {
        _components.Clear();
    }

    /// <summary>
    /// Get a component.
    /// </summary>
    public T? Get<T>() where T : Component
    {
        return _components.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Enable or disable all components.
    /// </summary>
    public void SetActive(bool active)
    {
        foreach (var component in _components)
        {
            component.SetActive(active);
        }
    }
}
