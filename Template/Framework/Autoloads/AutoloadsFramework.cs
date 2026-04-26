using __TEMPLATE__.Debugging;
using __TEMPLATE__.Ui;
using __TEMPLATE__.Ui.Console;
using Godot;
using System;
using System.Threading.Tasks;
using System.Linq;

#if DEBUG
using GodotUtils.Debugging;
#endif

namespace __TEMPLATE__;

/// <summary>
/// Base autoload node that wires the runtime framework lifecycle and shared services.
/// </summary>
public abstract partial class AutoloadsFramework : Node
{
    // Exports
    [Export] private MenuScenes _scenes = null!;

    /// <summary>
    /// Raised before the application quits so subscribers can perform asynchronous cleanup.
    /// </summary>
    public event Func<Task>? PreQuit;

    /// <summary>
    /// Gets the manager responsible for attaching and resolving game components.
    /// </summary>
    public GameComponentManager ComponentManager { get; private set; } = null!;

    /// <summary>
    /// Gets the in-game developer console instance.
    /// </summary>
    public GameConsole GameConsole { get; private set; } = null!;

    /// <summary>
    /// Gets the audio manager used for playback and mixer control.
    /// </summary>
    public AudioManager AudioManager { get; private set; } = null!;

    /// <summary>
    /// Gets the options manager used to load, persist, and update settings.
    /// </summary>
    public OptionsManager OptionsManager { get; private set; } = null!;

    /// <summary>
    /// Gets the scoped service provider used by framework systems.
    /// </summary>
    public Services Services { get; private set; } = null!;

    /// <summary>
    /// Gets the metrics overlay used for runtime instrumentation.
    /// </summary>
    public IMetricsOverlay Metrics { get; private set; } = null!;

    /// <summary>
    /// Gets the scene manager used to switch and coordinate scenes.
    /// </summary>
    public SceneManager SceneManager { get; private set; } = null!;

    /// <summary>
    /// Gets the manager that renders focus outlines for UI controls.
    /// </summary>
    public FocusOutlineManager FocusOutline { get; private set; } = null!;

    /// <summary>
    /// Gets the logger used for structured game and console messages.
    /// </summary>
    public Logger Logger { get; private set; } = null!;

    /// <summary>
    /// Gets the application lifetime service used to coordinate shutdown.
    /// </summary>
    public IApplicationLifetime ApplicationLifetime { get; private set; } = null!;

    /// <summary>
    /// Gets the tracker for long-running background tasks.
    /// </summary>
    public IBackgroundTaskTracker BackgroundTasks { get; private set; } = null!;

    /// <summary>
    /// Gets the immutable runtime service bundle exposed through <see cref="Game"/>.
    /// </summary>
    public GameServices RuntimeServices { get; private set; } = null!;

#if DEBUG
    private VisualizeAutoload _visualizeAutoload = null!;
#endif

    /// <summary>
    /// Called when the node enters the SceneTree.
    /// </summary>
    protected abstract void EnterTree();

    /// <summary>
    /// Called when the node and all of its children are ready.
    /// </summary>
    protected abstract void Ready();

    /// <summary>
    /// Called on each idle frame, prior to rendering, and after physics ticks have been processed.
    /// </summary>
    protected abstract void Process(double delta);

    /// <summary>
    /// Called once on each physics tick, and allows Nodes to synchronize their logic with physics ticks.
    /// </summary>
    protected abstract void PhysicsProcess(double delta);

    /// <summary>
    /// Called when the object receives a notification, which can be identified in what by comparing it with a constant.
    /// </summary>
    /// <param name="what">The Godot notification code.</param>
    protected abstract void Notification(int what);

    /// <summary>
    /// Called when the node exits the SceneTree.
    /// </summary>
    protected abstract void ExitTree();

    // Sealed Godot Overrides
    public sealed override void _EnterTree()
    {
        ComponentManager = GetNode<GameComponentManager>("ComponentManager");
        SceneManager = new SceneManager(this, _scenes);
        Services = new Services(this);
        MetricsOverlay metricsOverlay = new();
        Metrics = metricsOverlay;
        AddChild(metricsOverlay);
        GameConsole = GetNode<GameConsole>("%Console");
        FocusOutline = new FocusOutlineManager(this);
        Logger = new Logger(GameConsole);
        BackgroundTasks = new BackgroundTaskTracker(Logger);

        OptionsManager = OptionsManagerFactory.Create(this);
        AudioManager = new AudioManager(this, OptionsManager);
        ApplicationLifetime = new ApplicationLifetimeService(this);

        SceneManager.BindRuntimeServices(AudioManager, FocusOutline);
        Profiler.Configure(Metrics);

        RuntimeServices = new GameServices(
            ComponentManager,
            GameConsole,
            AudioManager,
            OptionsManager,
            Metrics,
            SceneManager,
            Services,
            FocusOutline,
            Logger,
            ApplicationLifetime,
            BackgroundTasks);
        Game.Initialize(RuntimeServices);

        SceneComposition.ConfigureNodeTree(this, RuntimeServices);
        SceneComposition.ConfigureNodeTree(SceneManager.CurrentScene, RuntimeServices);

        EnterTree();
    }

    public sealed override void _Ready()
    {
        Commands.RegisterAll(GameConsole, Logger, ApplicationLifetime);
        CommandLineArgs.Initialize();

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
        // Route window-close notifications to graceful shutdown flow.
        if (what == NotificationWMCloseRequest)
            BackgroundTasks.Run(_ => ExitGame(), "Autoloads.ExitGame");

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
        BackgroundTasks.Dispose();
        Profiler.Dispose();

        Game.Reset();

        ExitTree();
    }

    /// <summary>
    /// Deferred-call proxy used by Godot to switch scenes from a queued invocation.
    /// </summary>
    /// <param name="rawName">Raw scene name identifier.</param>
    /// <param name="transTypeVariant">Transition payload passed through as a variant.</param>
    public void DeferredSwitchSceneProxy(string rawName, Variant transTypeVariant)
    {
        SceneManager.DeferredSwitchScene(rawName, transTypeVariant);
    }

    /// <summary>
    /// Executes orderly shutdown by invoking <see cref="PreQuit"/> subscribers, then quits the tree.
    /// </summary>
    /// <returns>A task that completes after all <see cref="PreQuit"/> subscribers have finished and quit is requested.</returns>
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
                catch (OperationCanceledException ex)
                {
                    GD.Print($"PreQuit subscriber canceled: {ex.Message}");
                }
                catch (Exception ex) when (ExceptionGuard.IsNonFatal(ex))
                {
                    GD.PrintErr($"PreQuit subscriber failed: {ex}");
                }
            }
        }

        GetTree().Quit();
    }
}
