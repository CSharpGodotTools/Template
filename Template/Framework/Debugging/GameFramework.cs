using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;
using System;

namespace __TEMPLATE__;

public partial class Game
{
    private static GameServices? _services;

    private static GameServices RuntimeServices =>
        _services ?? throw new InvalidOperationException("Game services are not initialized yet.");

    internal static void Initialize(GameServices services)
    {
        _services = services;
    }

    internal static void Reset()
    {
        _services = null;
    }

    public static GameComponentManager ComponentManager => RuntimeServices.ComponentManager;
    public static GameConsole Console => RuntimeServices.GameConsole;
    public static GameConsole GameConsole => RuntimeServices.GameConsole;

    public static IAudioService Audio => RuntimeServices.Audio;
    public static AudioManager AudioManager => RuntimeServices.AudioManager;

    public static IOptionsService Options => RuntimeServices.Options;
    public static OptionsManager OptionsManager => RuntimeServices.OptionsManager;
    public static ResourceOptions Settings => RuntimeServices.Options.Settings;

    public static IMetricsOverlay Metrics => RuntimeServices.Metrics;

    public static ISceneService Scene => RuntimeServices.Scene;
    public static SceneManager SceneManager => RuntimeServices.SceneManager;

    public static Profiler Profiler => RuntimeServices.Profiler;
    public static Services Services => RuntimeServices.ScopedServices;
    public static Services ScopedServices => RuntimeServices.ScopedServices;

    public static FocusOutlineManager FocusOutline => RuntimeServices.FocusOutline;

    public static ILoggerService Logger => RuntimeServices.Logger;
    public static Logger LoggerManager => RuntimeServices.LoggerManager;

    public static IApplicationLifetime Application => RuntimeServices.ApplicationLifetime;
}
