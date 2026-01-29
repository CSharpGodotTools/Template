using __TEMPLATE__.Debugging;
using __TEMPLATE__.UI;
using __TEMPLATE__.UI.Console;
using Godot;
using GodotUtils;
using System;
using System.Threading.Tasks;

#if DEBUG
using GodotUtils.Debugging;
#endif

namespace __TEMPLATE__;

// Autoload
// Access the managers that live in here through through Game.(...)
// Alternatively access through GetNode<Autoloads>("/root/Autoloads")
public partial class Autoloads : Node
{
    #region Exports
    [Export] private MenuScenes _scenes;
    #endregion

    #region Events
    public event Func<Task> PreQuit;
    #endregion

    public static Autoloads Instance { get; private set; }

    #region Autoloads
    // Cannot use [Export] here because Godot will bug out and unlink export path in editor after setup completes and restarts the editor
    public GameComponentManager ComponentManager { get; private set; }
    public GameConsole          GameConsole      { get; private set; }
    public AudioManager         AudioManager     { get; private set; }
    public OptionsManager       OptionsManager   { get; private set; }
    public Services             Services         { get; private set; }
    public MetricsOverlay       MetricsOverlay   { get; private set; }
    public SceneManager         SceneManager     { get; private set; }
    public Profiler             Profiler         { get; private set; }
    public FocusOutlineManager  FocusOutline     { get; private set; }
    public Logger Logger { get; private set; }

#if DEBUG
    private VisualizeAutoload _visualizeAutoload;
#endif
    #endregion

    #region Godot Overrides
    public override void _EnterTree()
    {
        if (Instance != null)
            throw new InvalidOperationException($"{nameof(Autoloads)} has been initialized already. Did you try to run the Autoloads scene by itself?");

        Instance = this;
        ComponentManager = GetNode<GameComponentManager>("ComponentManager");
        SceneManager = new SceneManager(this, _scenes);
        Services = new Services(this);
        MetricsOverlay = new MetricsOverlay();
        Profiler = new Profiler();
        GameConsole = GetNode<GameConsole>("%Console");
        FocusOutline = new FocusOutlineManager(this);
        Logger = new Logger(GameConsole);
    }

    public override void _Ready()
    {
        CommandLineArgs.Initialize();
        Commands.RegisterAll();

        OptionsManager = new OptionsManager(this);
        AudioManager = new AudioManager(this);

#if DEBUG
        _visualizeAutoload = new VisualizeAutoload();
#endif
    }

    public override void _Process(double delta)
    {
        OptionsManager.Update();
        MetricsOverlay.Update();

#if DEBUG
        _visualizeAutoload.Update();
#endif

        Logger.Update();
    }

    public override void _PhysicsProcess(double delta)
    {
        MetricsOverlay.UpdatePhysics();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            ExitGame().FireAndForget();
        }
    }

    public override void _ExitTree()
    {
        AudioManager.Dispose();
        OptionsManager.Dispose();
        SceneManager.Dispose();

#if DEBUG
        _visualizeAutoload.Dispose();
#endif

        Logger.Dispose();
        Profiler.Dispose();

        Instance = null;
    }
    #endregion

    public void DeferredSwitchSceneProxy(string rawName, Variant transTypeVariant)
    {
        SceneManager.DeferredSwitchScene(rawName, transTypeVariant);
    }

    public async Task ExitGame()
    {
        GetTree().AutoAcceptQuit = false;

        // Wait for cleanup
        if (PreQuit != null)
        {
            // Since the PreQuit event contains a Task only the first subscriber will be invoked
            // with await PreQuit?.Invoke(); so need to ensure all subs are invoked.
            foreach (Func<Task> subscriber in PreQuit.GetInvocationList())
            {
                try
                {
                    await subscriber();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"PreQuit subscriber failed: {ex}");
                }
            }
        }

        GetTree().Quit();
    }
}
