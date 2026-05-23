using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;
using System;

namespace __TEMPLATE__;

public partial class Game
{
    /// <summary>
    /// Gets the global component manager.
    /// </summary>
    public static GameComponentManager ComponentManager => Framework.ComponentManager;

    /// <summary>
    /// Gets the global in-game console instance.
    /// </summary>
    public static GameConsole Console => Framework.GameConsole;

    /// <summary>
    /// Gets the audio service facade.
    /// </summary>
    public static IAudioService Audio => Framework.AudioManager;

    /// <summary>
    /// Gets the concrete audio manager.
    /// </summary>
    public static AudioManager AudioManager => Framework.AudioManager;

    /// <summary>
    /// Gets the options service facade.
    /// </summary>
    public static IOptionsService Options => Framework.OptionsManager;

    /// <summary>
    /// Gets the concrete options manager.
    /// </summary>
    public static OptionsManager OptionsManager => Framework.OptionsManager;

    /// <summary>
    /// Gets the runtime metrics overlay.
    /// </summary>
    public static IMetricsOverlay Metrics => Framework.Metrics;

    /// <summary>
    /// Gets the scene service facade.
    /// </summary>
    public static ISceneService Scene => Framework.SceneManager;

    /// <summary>
    /// Gets the concrete scene manager.
    /// </summary>
    public static SceneManager SceneManager => Framework.SceneManager;

    /// <summary>
    /// Gets the scoped services container.
    /// </summary>
    public static Services Services => Framework.Services;

    /// <summary>
    /// Gets the focus outline manager used by UI systems.
    /// </summary>
    public static FocusOutlineManager FocusOutline => Framework.FocusOutline;

    /// <summary>
    /// Gets the logger service.
    /// </summary>
    public static ILoggerService Logger => Framework.Logger;

    /// <summary>
    /// Gets the concrete logger manager.
    /// </summary>
    public static Logger LoggerManager => Framework.Logger;

    /// <summary>
    /// Gets the background task tracker.
    /// </summary>
    public static IBackgroundTaskTracker BackgroundTasks => Framework.BackgroundTasks;

    /// <summary>
    /// Gets the application lifetime service.
    /// </summary>
    public static IApplicationLifetime Application => Framework.ApplicationLifetime;

    /// <summary>
    /// Profiler for logging runtime of code.
    /// </summary>
    public static IProfiler Profiler => Framework.Profiler;

    /// <summary>
    /// Attempts to retrieve initialized framework services.
    /// </summary>
    /// <param name="framework">Resolved framework instance when available.</param>
    /// <returns><see langword="true"/> when services are initialized; otherwise <see langword="false"/>.</returns>
    internal static bool TryGetFramework(out AutoloadsFramework framework)
    {
        // Return false when runtime services have not been initialized yet.
        if (_framework is null)
        {
            framework = null!;
            return false;
        }

        framework = _framework;
        return true;
    }

    internal static IGameLifecycle Lifecycle { get; } = new GameLifecycle();

    internal interface IGameLifecycle
    {
        void Initialize(AutoloadsFramework framework);
        void Reset();
    }

    private sealed class GameLifecycle : IGameLifecycle
    {
        void IGameLifecycle.Initialize(AutoloadsFramework framework)
        {
            ArgumentNullException.ThrowIfNull(framework);
            _framework = framework;
        }

        void IGameLifecycle.Reset()
        {
            _framework = null;
        }
    }

    private static AutoloadsFramework? _framework;

    private static AutoloadsFramework Framework =>
        _framework ?? throw new InvalidOperationException("Game services are not initialized yet.");
}
