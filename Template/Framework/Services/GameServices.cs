using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;

namespace __TEMPLATE__;

/// <summary>
/// Aggregates core runtime services used by gameplay systems.
/// </summary>
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
    IBackgroundTaskTracker backgroundTasks,
    Profiler profiler)
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

    /// <summary>
    /// Profiler service.
    /// </summary>
    public IProfiler Profiler { get; } = profiler;

    // Concrete convenience accessors for transition compatibility.
    public AudioManager AudioManager => (AudioManager)Audio;
    public OptionsManager OptionsManager => (OptionsManager)Options;
    public SceneManager SceneManager => (SceneManager)Scene;
    public Logger LoggerManager => (Logger)Logger;
}
