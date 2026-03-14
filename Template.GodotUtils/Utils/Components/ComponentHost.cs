using System.Collections.Generic;
using System.Linq;

namespace GodotUtils;

/// <summary>
/// Holds and manages a set of components.
/// </summary>
public class ComponentHost
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
