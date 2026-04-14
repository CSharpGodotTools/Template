using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;

namespace __TEMPLATE__;

/// <summary>
/// Aggregates core runtime services used by gameplay systems.
/// </summary>
/// <param name="componentManager">Component manager service.</param>
/// <param name="gameConsole">Game console service.</param>
/// <param name="audio">Audio service abstraction.</param>
/// <param name="options">Options service abstraction.</param>
/// <param name="metrics">Metrics overlay service.</param>
/// <param name="scene">Scene service abstraction.</param>
/// <param name="scopedServices">Container for scoped services.</param>
/// <param name="focusOutline">Focus-outline manager.</param>
/// <param name="logger">Logger service abstraction.</param>
/// <param name="applicationLifetime">Application lifetime service.</param>
/// <param name="backgroundTasks">Background task tracker service.</param>
public sealed class GameServices(
    GameComponentManager componentManager,
    GameConsole gameConsole,
    IAudioService audio,
    IOptionsService options,
    IMetricsOverlay metrics,
    ISceneService scene,
    Services scopedServices,
    FocusOutlineManager focusOutline,
    ILoggerService logger,
    IApplicationLifetime applicationLifetime,
    IBackgroundTaskTracker backgroundTasks)
{
    /// <summary>
    /// Component manager service.
    /// </summary>
    public GameComponentManager ComponentManager { get; } = componentManager;

    /// <summary>
    /// Game console service.
    /// </summary>
    public GameConsole GameConsole { get; } = gameConsole;

    /// <summary>
    /// Audio service abstraction.
    /// </summary>
    public IAudioService Audio { get; } = audio;

    /// <summary>
    /// Options service abstraction.
    /// </summary>
    public IOptionsService Options { get; } = options;

    /// <summary>
    /// Metrics overlay service.
    /// </summary>
    public IMetricsOverlay Metrics { get; } = metrics;

    /// <summary>
    /// Scene service abstraction.
    /// </summary>
    public ISceneService Scene { get; } = scene;

    /// <summary>
    /// Scoped services container.
    /// </summary>
    public Services ScopedServices { get; } = scopedServices;

    /// <summary>
    /// Focus-outline manager.
    /// </summary>
    public FocusOutlineManager FocusOutline { get; } = focusOutline;

    /// <summary>
    /// Logger service abstraction.
    /// </summary>
    public ILoggerService Logger { get; } = logger;

    /// <summary>
    /// Application lifetime service.
    /// </summary>
    public IApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;

    /// <summary>
    /// Background task tracker service.
    /// </summary>
    public IBackgroundTaskTracker BackgroundTasks { get; } = backgroundTasks;

    // Concrete convenience accessors for transition compatibility.
    public AudioManager AudioManager => (AudioManager)Audio;
    public OptionsManager OptionsManager => (OptionsManager)Options;
    public SceneManager SceneManager => (SceneManager)Scene;
    public Logger LoggerManager => (Logger)Logger;
}
