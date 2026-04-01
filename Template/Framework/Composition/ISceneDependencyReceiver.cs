namespace __TEMPLATE__;

/// <summary>
/// Represents a scene node that can receive framework runtime services after instantiation.
/// </summary>
public interface ISceneDependencyReceiver
{
    /// <summary>
    /// Injects runtime services required by the receiver.
    /// </summary>
    /// <param name="services">Service bundle exposed by the game framework.</param>
    void Configure(GameServices services);
}
