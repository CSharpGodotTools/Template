namespace GodotUtils;

/// <summary>
/// Defines component host operations.
/// </summary>
public interface IComponentHost
{
    /// <summary>
    /// Adds a component to the host.
    /// </summary>
    void Add(Component component);

    /// <summary>
    /// Sets active state on all hosted components.
    /// </summary>
    void SetActive(bool active);
}
