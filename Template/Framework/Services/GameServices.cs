using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;

namespace __TEMPLATE__;

public sealed class GameServices(
    GameComponentManager componentManager,
    GameConsole gameConsole,
    IAudioService audio,
    IOptionsService options,
    IMetricsOverlay metrics,
    ISceneService scene,
    Profiler profiler,
    Services scopedServices,
    FocusOutlineManager focusOutline,
    ILoggerService logger,
    IApplicationLifetime applicationLifetime,
    IBackgroundTaskTracker backgroundTasks)
{
    public GameComponentManager ComponentManager { get; } = componentManager;
    public GameConsole GameConsole { get; } = gameConsole;
    public IAudioService Audio { get; } = audio;
    public IOptionsService Options { get; } = options;
    public IMetricsOverlay Metrics { get; } = metrics;
    public ISceneService Scene { get; } = scene;
    public Profiler Profiler { get; } = profiler;
    public Services ScopedServices { get; } = scopedServices;
    public FocusOutlineManager FocusOutline { get; } = focusOutline;
    public ILoggerService Logger { get; } = logger;
    public IApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;
    public IBackgroundTaskTracker BackgroundTasks { get; } = backgroundTasks;

    // Concrete convenience accessors for transition compatibility.
    public AudioManager AudioManager => (AudioManager)Audio;
    public OptionsManager OptionsManager => (OptionsManager)Options;
    public SceneManager SceneManager => (SceneManager)Scene;
    public Logger LoggerManager => (Logger)Logger;
}
