using Framework.Debugging;
using Framework.UI;
using Framework.UI.Console;
using System;

namespace Framework;

public partial class GameFramework
{
#if DEBUG
    /// <summary>
    /// Check if the autoloads singleton is not null. If it is null then show an error explaining
    /// that the developer cannot access the autoloads singleton just yet and it needs time to
    /// setup.
    /// </summary>
    private static T IsAutoloadsSetup<T>(Func<Autoloads, T> getPropertyFrom, string propertyName) where T : class
    {
        Autoloads autoloads = Autoloads.Instance;

        if (autoloads == null)
        {
            string errMsg = $"Game.{propertyName} was accessed before _EnterTree or _Ready.";

            // Show a friendly optional error message if netcode is enabled.
            // Since Autoloads is null we can make our own temporary Logger.
            Logger logger = new();
            logger.LogDebug(errMsg + " (see exception in errors for stack trace)");
            logger.Update();

            throw new InvalidOperationException(errMsg);
        }

        return getPropertyFrom(autoloads)!; // Assumes the field may be null, but we are not checking it here
    }

    public static FocusOutlineManager FocusOutline => IsAutoloadsSetup(a => a.FocusOutline, nameof(FocusOutline));
    public static MetricsOverlay Metrics => IsAutoloadsSetup(a => a.MetricsOverlay, nameof(Metrics));
    public static OptionsManager Options => IsAutoloadsSetup(a => a.OptionsManager, nameof(Options));
    public static AudioManager Audio => IsAutoloadsSetup(a => a.AudioManager, nameof(Audio));
    public static SceneManager Scene => IsAutoloadsSetup(a => a.SceneManager, nameof(Scene));
    public static GameConsole Console => IsAutoloadsSetup(a => a.GameConsole, nameof(Console));
    public static Profiler Profiler => IsAutoloadsSetup(a => a.Profiler, nameof(Profiler));
    public static Services Services => IsAutoloadsSetup(a => a.Services, nameof(Services));
    public static Logger Logger => IsAutoloadsSetup(a => a.Logger, nameof(Logger));
#else
    // The games release will not have the slow debugging checks
    public static FocusOutlineManager FocusOutline => Autoloads.Instance.FocusOutline;
    public static MetricsOverlay      Metrics      => Autoloads.Instance.MetricsOverlay;
    public static OptionsManager      Options      => Autoloads.Instance.OptionsManager;
    public static AudioManager        Audio        => Autoloads.Instance.AudioManager;
    public static SceneManager        Scene        => Autoloads.Instance.SceneManager;
    public static GameConsole         Console      => Autoloads.Instance.GameConsole;
    public static Profiler            Profiler     => Autoloads.Instance.Profiler;
    public static Services            Services     => Autoloads.Instance.Services;
    public static Logger              Logger       => Autoloads.Instance.Logger;
#endif
}
