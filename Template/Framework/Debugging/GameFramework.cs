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
    public static GameComponentManager ComponentManager => RuntimeServices.ComponentManager;

    /// <summary>
    /// Gets the global in-game console instance.
    /// </summary>
    public static GameConsole Console => RuntimeServices.GameConsole;

    /// <summary>
    /// Gets the audio service facade.
    /// </summary>
    public static IAudioService Audio => RuntimeServices.Audio;

    /// <summary>
    /// Gets the options service facade.
    /// </summary>
    public static IOptionsService Options => RuntimeServices.Options;

    /// <summary>
    /// Gets the runtime metrics overlay.
    /// </summary>
    public static IMetricsOverlay Metrics => RuntimeServices.Metrics;

    /// <summary>
    /// Gets the scene service facade.
    /// </summary>
    public static ISceneService Scene => RuntimeServices.Scene;

    /// <summary>
    /// Gets the global profiler.
    /// </summary>
    public static Profiler Profiler => RuntimeServices.Profiler;

    /// <summary>
    /// Gets the scoped services container.
    /// </summary>
    public static Services Services => RuntimeServices.ScopedServices;

    /// <summary>
    /// Gets the focus outline manager used by UI systems.
    /// </summary>
    public static FocusOutlineManager FocusOutline => RuntimeServices.FocusOutline;

    /// <summary>
    /// Gets the logger service.
    /// </summary>
    public static ILoggerService Logger => RuntimeServices.Logger;

    /// <summary>
    /// Gets the background task tracker.
    /// </summary>
    public static IBackgroundTaskTracker BackgroundTasks => RuntimeServices.BackgroundTasks;

    /// <summary>
    /// Gets the application lifetime service.
    /// </summary>
    public static IApplicationLifetime Application => RuntimeServices.ApplicationLifetime;

    /// <summary>
    /// Initializes the global runtime service bundle.
    /// </summary>
    /// <param name="services">Service bundle to expose globally.</param>
    internal static void Initialize(GameServices services)
    {
        _services = services;
    }

    /// <summary>
    /// Attempts to retrieve initialized runtime services.
    /// </summary>
    /// <param name="services">Resolved services when available.</param>
    /// <returns><see langword="true"/> when services are initialized; otherwise <see langword="false"/>.</returns>
    internal static bool TryGetServices(out GameServices services)
    {
        // Return false when runtime services have not been initialized yet.
        if (_services is null)
        {
            services = null!;
            return false;
        }

        services = _services;
        return true;
    }

    /// <summary>
    /// Clears global runtime services during framework shutdown.
    /// </summary>
    internal static void Reset()
    {
        _services = null;
    }

    private static GameServices? _services;

    private static GameServices RuntimeServices =>
        _services ?? throw new InvalidOperationException("Game services are not initialized yet.");
}
