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
    /// Adds a component to the set.
    /// </summary>
    /// <param name="component">Component to add.</param>
    public void Add(Component component)
    {
        _components.Add(component);
    }

    /// <summary>
    /// Adds all components from another list.
    /// </summary>
    /// <param name="components">Source list to merge from.</param>
    public void Add(ComponentList components)
    {
        foreach (Component component in components._components)
            _components.Add(component);
    }

    /// <summary>
    /// Removes a component from the set.
    /// </summary>
    /// <param name="component">Component to remove.</param>
    public void Remove(Component component)
    {
        _components.Remove(component);
    }

    /// <summary>
    /// Removes all components from the set.
    /// </summary>
    public void RemoveAll()
    {
        _components.Clear();
    }

    /// <summary>
    /// Returns the first component of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Component type to locate.</typeparam>
    /// <returns>Matching component, or <see langword="null"/> when absent.</returns>
    public T? Get<T>() where T : Component
    {
        return _components.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Enables or disables all tracked components.
    /// </summary>
    /// <param name="active">Desired active state.</param>
    public void SetActive(bool active)
    {
        foreach (var component in _components)
            component.SetActive(active);
    }
}
