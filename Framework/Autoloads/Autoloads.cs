using Godot;
using GodotUtils.UI;
using GodotUtils.UI.Console;
#if DEBUG
using GodotUtils.Debugging;
#endif
using System;
using System.Threading.Tasks;

namespace GodotUtils;

// Autoload
// Access this with GetNode<Autoloads>("/root/Autoloads")
public partial class Autoloads : Node
{
    [Export] private MenuScenes _scenes;

    public event Func<Task> PreQuit;

    public static Autoloads Instance { get; private set; }

    // Game developers should be able to access each individual manager
    public ComponentManager ComponentManager { get; private set; }
    public AudioManager     AudioManager     { get; private set; }
    public OptionsManager   OptionsManager   { get; private set; }
    public Services         Services         { get; private set; }
    public MetricsOverlay   MetricsOverlay   { get; private set; }
    public SceneManager     SceneManager     { get; private set; }
    public GameConsole      GameConsole      { get; private set; }

#if NETCODE_ENABLED
    private Logger _logger;
#endif

#if DEBUG
    private VisualizeAutoload _visualizeAutoload;
#endif

    public override void _EnterTree()
    {
        if (Instance != null)
            throw new InvalidOperationException("Global has been initialized already");

        Instance = this;
        ComponentManager = GetNode<ComponentManager>("ComponentManager");
        GameConsole = GetNode<GameConsole>("%Console");
        SceneManager = new SceneManager(this, _scenes);
        Services = new Services(this);
        MetricsOverlay = new MetricsOverlay();

#if NETCODE_ENABLED
        _logger = new Logger(GameConsole);
#endif
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

#if NETCODE_ENABLED
        _logger.Update();
#endif
    }

    public override void _PhysicsProcess(double delta)
    {
        MetricsOverlay.UpdatePhysics();
    }

    public override async void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            await QuitAndCleanup();
        }
    }

    public override void _ExitTree()
    {
        AudioManager.Dispose();
        OptionsManager.Dispose();
        SceneManager.Dispose();
        Services.Dispose();
        MetricsOverlay.Dispose();

#if DEBUG
        _visualizeAutoload.Dispose();
#endif

#if NETCODE_ENABLED
        _logger.Dispose();
#endif

        Profiler.Dispose();

        Instance = null;
    }

    // Using deferred is always complicated...
    public void DeferredSwitchSceneProxy(string rawName, Variant transTypeVariant)
    {
        if (SceneManager.Instance == null)
            return;

        SceneManager.Instance.DeferredSwitchScene(rawName, transTypeVariant);
    }

    public async Task QuitAndCleanup()
    {
        GetTree().AutoAcceptQuit = false;

        // Wait for cleanup
        if (PreQuit != null)
        {
            // Since the PreQuit event contains a Task only the first subscriber will be invoked
            // with await PreQuit?.Invoke(); so need to ensure all subs are invoked.
            Delegate[] invocationList = PreQuit.GetInvocationList();
            foreach (Func<Task> subscriber in invocationList)
            {
                await subscriber();
            }
        }

        GetTree().Quit();
    }
}
