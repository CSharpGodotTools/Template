using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Holds and manages a set of components.
/// </summary>
public class ComponentHost : IComponentHost
{
    private readonly List<Component> _components = [];

    /// <summary>
    /// Adds a component to the host.
    /// </summary>
    public void Add(Component component)
    {
        _components.Add(component);
    }

    /// <summary>
    /// Sets active state on all hosted components.
    /// </summary>
    public void SetActive(bool active)
    {
        for (int i = 0; i < _components.Count; i++)
        {
            _components[i].SetActive(active);
        }
    }
}
