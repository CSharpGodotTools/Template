using Framework.Debugging;
using Framework.UI;
using Framework.UI.Console;
using System;

namespace Framework;

public partial class GameFramework
{
    public static FocusOutlineManager FocusOutline => Autoloads.Instance.FocusOutline;
    public static MetricsOverlay      Metrics      => Autoloads.Instance.MetricsOverlay;
    public static OptionsManager      Options      => Autoloads.Instance.OptionsManager;
    public static ResourceOptions     Settings     => Autoloads.Instance.OptionsManager.Settings;
    public static AudioManager        Audio        => Autoloads.Instance.AudioManager;
    public static SceneManager        Scene        => Autoloads.Instance.SceneManager;
    public static GameConsole         Console      => Autoloads.Instance.GameConsole;
    public static Profiler            Profiler     => Autoloads.Instance.Profiler;
    public static Services            Services     => Autoloads.Instance.Services;
    public static Logger              Logger       => Autoloads.Instance.Logger;
}
