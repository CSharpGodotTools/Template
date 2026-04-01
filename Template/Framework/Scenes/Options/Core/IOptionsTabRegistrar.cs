namespace __TEMPLATE__.Ui;

/// <summary>
/// Defines a component that registers option entries for a single tab.
/// </summary>
public interface IOptionsTabRegistrar
{
    /// <summary>
    /// Gets the tab name this registrar contributes to.
    /// </summary>
    string TabName { get; }

    /// <summary>
    /// Registers this registrar's options with the provided service.
    /// </summary>
    /// <param name="optionsService">Options service used for registration.</param>
    void Register(IOptionsService optionsService);
}
