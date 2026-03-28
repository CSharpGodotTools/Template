using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;
using System;

namespace __TEMPLATE__;

public partial class Game
{
    public static GameComponentManager ComponentManager => RuntimeServices.ComponentManager;
    public static GameConsole Console => RuntimeServices.GameConsole;

    public static IAudioService Audio => RuntimeServices.Audio;
    public static IOptionsService Options => RuntimeServices.Options;
    public static IMetricsOverlay Metrics => RuntimeServices.Metrics;
    public static ISceneService Scene => RuntimeServices.Scene;

    public static Profiler Profiler => RuntimeServices.Profiler;
    public static Services Services => RuntimeServices.ScopedServices;

    public static FocusOutlineManager FocusOutline => RuntimeServices.FocusOutline;

    public static ILoggerService Logger => RuntimeServices.Logger;

    public static IApplicationLifetime Application => RuntimeServices.ApplicationLifetime;

    internal static void Initialize(GameServices services)
    {
        _services = services;
    }

    internal static bool TryGetServices(out GameServices services)
    {
        if (_services is null)
        {
            services = null!;
            return false;
        }

        services = _services;
        return true;
    }

    internal static void Reset()
    {
        _services = null;
    }

    private static GameServices? _services;

    private static GameServices RuntimeServices =>
        _services ?? throw new InvalidOperationException("Game services are not initialized yet.");
}
