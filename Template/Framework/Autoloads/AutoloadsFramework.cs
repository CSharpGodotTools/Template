using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;
using Godot;
using GodotUtils;
using System;
using System.Threading.Tasks;
using System.Linq;


#if DEBUG
using GodotUtils.Debugging;
#endif

namespace __TEMPLATE__;

// Autoload
// Access runtime services via Game.*.
// Alternatively access through GetNode<Autoloads>("/root/Autoloads")
public abstract partial class AutoloadsFramework : Node
{
    // Exports
    [Export] private MenuScenes _scenes = null!;

    // Events
    public event Func<Task>? PreQuit;

    // Autoloads
    // Access runtime services via Game.
    public GameComponentManager ComponentManager { get; private set; } = null!;
    public GameConsole GameConsole { get; private set; } = null!;
    public AudioManager AudioManager { get; private set; } = null!;
    public OptionsManager OptionsManager { get; private set; } = null!;
    public Services Services { get; private set; } = null!;
    public IMetricsOverlay Metrics { get; private set; } = null!;
    public SceneManager SceneManager { get; private set; } = null!;
    public Profiler Profiler { get; private set; } = null!;
    public FocusOutlineManager FocusOutline { get; private set; } = null!;
    public Logger Logger { get; private set; } = null!;
    public IApplicationLifetime ApplicationLifetime { get; private set; } = null!;
    public GameServices RuntimeServices { get; private set; } = null!;

#if DEBUG
    private VisualizeAutoload _visualizeAutoload = null!;
#endif

    protected abstract void EnterTree();
    protected abstract void Ready();
    protected abstract void Process(double delta);
    protected abstract void PhysicsProcess(double delta);
    protected abstract void Notification(int what);
    protected abstract void ExitTree();

    // Godot Overrides
    public sealed override void _EnterTree()
    {
        ComponentManager = GetNode<GameComponentManager>("ComponentManager");
        SceneManager = new SceneManager(this, _scenes);
        Services = new Services(this);
        MetricsOverlay metricsOverlay = new();
        Metrics = metricsOverlay;
        AddChild(metricsOverlay);
        Profiler = new Profiler();
        GameConsole = GetNode<GameConsole>("%Console");
        FocusOutline = new FocusOutlineManager(this);
        Logger = new Logger(GameConsole);

        EnterTree();
    }

    public sealed override void _Ready()
    {
        Commands.RegisterAll();

        OptionsManager = new OptionsManager(this);
        AudioManager = new AudioManager(this, OptionsManager);
        CommandLineArgs.Initialize();
        ApplicationLifetime = new ApplicationLifetimeService(this);

        RuntimeServices = new GameServices(
            ComponentManager,
            GameConsole,
            AudioManager,
            OptionsManager,
            Metrics,
            SceneManager,
            Profiler,
            Services,
            FocusOutline,
            Logger,
            ApplicationLifetime);
        Game.Initialize(RuntimeServices);

#if DEBUG
        _visualizeAutoload = new VisualizeAutoload();
#endif

        Ready();
    }

    public sealed override void _Process(double delta)
    {
        OptionsManager.Update();

#if DEBUG
        Visualize.Update();
#endif

        Logger.Update();

        Process(delta);
    }

    public sealed override void _PhysicsProcess(double delta)
    {
        PhysicsProcess(delta);
    }

    public sealed override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            TaskUtils.FireAndForget(ExitGame);
        }

        Notification(what);
    }

    public sealed override void _ExitTree()
    {
        AudioManager.Dispose();
        OptionsManager.Dispose();
        SceneManager.Dispose();

#if DEBUG
        _visualizeAutoload.Dispose();
#endif

        Logger.Dispose();
        Profiler.Dispose();

        Game.Reset();

        ExitTree();
    }

    // Special Proxy Method for Usage of Deferred
    public void DeferredSwitchSceneProxy(string rawName, Variant transTypeVariant)
    {
        SceneManager.DeferredSwitchScene(rawName, transTypeVariant);
    }

    // ExitGame
    public async Task ExitGame()
    {
        GetTree().AutoAcceptQuit = false;

        // Wait for cleanup
        if (PreQuit != null)
        {
            // Since the PreQuit event contains a Task only the first subscriber will be invoked
            // with await PreQuit?.Invoke(); so need to ensure all subs are invoked.
            foreach (Func<Task> subscriber in PreQuit.GetInvocationList().Cast<Func<Task>>())
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
