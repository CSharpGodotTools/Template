using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;

namespace __TEMPLATE__;

public partial class Game
{
    private static GameServices _services = null!;

    internal static void Initialize(GameServices services)
    {
        _services = services;
    }

    internal static void Reset()
    {
        _services = null!;
    }

    public static GameComponentManager ComponentManager => _services.ComponentManager;
    public static GameConsole Console => _services.GameConsole;
    public static GameConsole GameConsole => _services.GameConsole;

    public static IAudioService Audio => _services.Audio;
    public static AudioManager AudioManager => _services.AudioManager;

    public static IOptionsService Options => _services.Options;
    public static OptionsManager OptionsManager => _services.OptionsManager;
    public static ResourceOptions Settings => _services.Options.Settings;

    public static IMetricsOverlay Metrics => _services.Metrics;

    public static ISceneService Scene => _services.Scene;
    public static SceneManager SceneManager => _services.SceneManager;

    public static Profiler Profiler => _services.Profiler;
    public static Services Services => _services.ScopedServices;
    public static Services ScopedServices => _services.ScopedServices;

    public static FocusOutlineManager FocusOutline => _services.FocusOutline;

    public static ILoggerService Logger => _services.Logger;
    public static Logger LoggerManager => _services.LoggerManager;

    public static IApplicationLifetime Application => _services.ApplicationLifetime;
}
