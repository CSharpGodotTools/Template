using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;

namespace __TEMPLATE__;

public partial class Game
{
    public static FocusOutlineManager FocusOutline => Autoloads.Instance!.FocusOutline;
    public static IMetricsOverlay     Metrics      => Autoloads.Instance!.Metrics;
    public static OptionsManager      Options      => Autoloads.Instance!.OptionsManager;
    public static ResourceOptions     Settings     => Autoloads.Instance!.OptionsManager.Settings;
    public static AudioManager        Audio        => Autoloads.Instance!.AudioManager;
    public static SceneManager        Scene        => Autoloads.Instance!.SceneManager;
    public static GameConsole         Console      => Autoloads.Instance!.GameConsole;
    public static Profiler            Profiler     => Autoloads.Instance!.Profiler;
    public static Services            Services     => Autoloads.Instance!.Services;
    public static Logger              Logger       => Autoloads.Instance!.Logger;
}
