using Godot;
using GodotUtils;
using System;
using System.Threading.Tasks;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Provides shared main-menu navigation behavior and scene transitions.
/// </summary>
public abstract partial class MainMenuNavFramework : Node, ISceneDependencyReceiver
{
    // Exports
    [Export] private PackedScene _gameScene = null!;

    // Fields
    private SceneManager _scene = null!;
    private FocusOutlineManager _focusOutline = null!;
    private IApplicationLifetime _applicationLifetime = null!;
    private bool _isConfigured;
    private Viewport _viewport = null!;
    private Button _playBtn = null!;
    private bool _focusWasNeverChanged = true;

    /// <summary>
    /// Injects runtime services required by the menu navigation framework.
    /// </summary>
    /// <param name="services">Resolved game service bundle.</param>
    public void Configure(GameServices services)
    {
        _scene = services.SceneManager;
        _focusOutline = services.FocusOutline;
        _applicationLifetime = services.ApplicationLifetime;
        _isConfigured = true;
    }

    // Godot Overrides
    public override void _EnterTree()
    {
        SceneComposition.ConfigureNodeFromGame(this);
    }

    public override void _Ready()
    {
        // Composition must inject runtime dependencies before scene readiness.
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(MainMenuNavFramework)} was not configured before _Ready.");

        _viewport = GetViewport();
        _playBtn = GetNode<Button>("Play");

        FocusOutlineOnPlayBtn();

        _scene.PostSceneChanged += OnPostSceneChanged;
        _viewport.GuiFocusChanged += OnGuiFocusChanged;
    }

    public override void _Input(InputEvent @event)
    {
        // Process menu-navigation key logic only for keyboard input events.
        if (@event is InputEventKey keyEvent)
        {
            // Solve the issue of pressing up key not focusing on play button if focus was never changed before
            if (keyEvent.IsJustPressed(Key.Up) && _focusWasNeverChanged)
                FocusOutlineOnPlayBtn();
        }
    }

    public override void _ExitTree()
    {
        _viewport.GuiFocusChanged -= OnGuiFocusChanged;
        _scene.PostSceneChanged -= OnPostSceneChanged;
    }

    protected abstract void Play();
    protected abstract void Mods();
    protected abstract void Options();
    protected abstract void Credits();
    protected abstract void Quit();

    private void FocusOutlineOnPlayBtn() => _focusOutline.Focus(_playBtn);

    /// <summary>
    /// Requests a graceful game shutdown.
    /// </summary>
    /// <returns>A task that completes when shutdown has been requested.</returns>
    private async Task ExitGameAsync()
    {
        try
        {
            await _applicationLifetime.ExitGameAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            GD.PrintErr($"Failed to exit game: {exception}");
        }
    }

    private void OnPlayPressed()
    {
        ArgumentNullException.ThrowIfNull(_gameScene, nameof(_gameScene));
        _scene.SwitchTo(_gameScene);
        Play();
    }

    private void OnGuiFocusChanged(Control _)
    {
        _focusWasNeverChanged = false;
    }

    private void OnModsPressed()
    {
        _scene.SwitchToModLoader();
        Mods();
    }

    private void OnOptionsPressed()
    {
        _scene.SwitchToOptions();
        Options();
    }

    private void OnCreditsPressed()
    {
        _scene.SwitchToCredits();
        Credits();
    }

    private void OnQuitPressed()
    {
        _ = ExitGameAsync();
    }

    private void OnPostSceneChanged()
    {
        FocusOutlineOnPlayBtn();
    }
}
