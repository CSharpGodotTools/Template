using __TEMPLATE__.Debugging;
using __TEMPLATE__.UI;
using __TEMPLATE__.UI.Console;

namespace __TEMPLATE__;

public static class Game
{
    public static MetricsOverlay Metrics  => Autoloads.Instance.MetricsOverlay;
    public static OptionsManager Options  => Autoloads.Instance.OptionsManager;
    public static AudioManager   Audio    => Autoloads.Instance.AudioManager;
    public static SceneManager   Scene    => Autoloads.Instance.SceneManager;
    public static GameConsole    Console  => Autoloads.Instance.GameConsole;
    public static Profiler       Profiler => Autoloads.Instance.Profiler;
    public static Services       Services => Autoloads.Instance.Services;
    public static Logger         Logger   => Autoloads.Instance.Logger;
}
