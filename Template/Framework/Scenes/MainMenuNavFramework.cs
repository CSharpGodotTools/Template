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

    public override void _EnterTree()
    {
        SceneComposition.ConfigureNodeFromGame(this);
    }

    // Godot Overrides
    public override void _Ready()
    {
        // Composition must inject runtime dependencies before scene readiness.
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(MainMenuNavFramework)} was not configured before _Ready.");

        _viewport = GetViewport();
        _playBtn = GetNode<Button>("Play");

        FocusOnPlayBtn();

        _scene.PostSceneChanged += OnPostSceneChanged;

        _viewport.GuiFocusChanged += OnGuiFocusChanged;
    }

    /// <summary>
    /// Tracks whether focus has changed since scene entry.
    /// </summary>
    /// <param name="node">Control that received focus.</param>
    private void OnGuiFocusChanged(Control node)
    {
        _focusWasNeverChanged = false;
    }

    public override void _Input(InputEvent @event)
    {
        // Process menu-navigation key logic only for keyboard input events.
        if (@event is InputEventKey keyEvent)
        {

            // Solve the issue of pressing up key not focusing on play button if focus was never changed before
            if (keyEvent.IsJustPressed(Key.Up) && _focusWasNeverChanged)
            {
                FocusOnPlayBtn();
            }
        }
    }

    public override void _ExitTree()
    {
        _viewport.GuiFocusChanged -= OnGuiFocusChanged;
        _scene.PostSceneChanged -= OnPostSceneChanged;
    }

    // FocusOnPlayBtn
    /// <summary>
    /// Moves keyboard/controller focus to the Play button.
    /// </summary>
    private void FocusOnPlayBtn()
    {
        _focusOutline.Focus(_playBtn);
    }

    // Abstract
    /// <summary>
    /// Invoked after navigating to the game scene.
    /// </summary>
    protected abstract void Play();

    /// <summary>
    /// Invoked after navigating to the mod loader scene.
    /// </summary>
    protected abstract void Mods();

    /// <summary>
    /// Invoked after navigating to the options scene.
    /// </summary>
    protected abstract void Options();

    /// <summary>
    /// Invoked after navigating to the credits scene.
    /// </summary>
    protected abstract void Credits();

    /// <summary>
    /// Invoked when the user chooses to quit.
    /// </summary>
    protected abstract void Quit();

    // Subscribers
    /// <summary>
    /// Handles Play button activation.
    /// </summary>
    private void OnPlayPressed()
    {
        ArgumentNullException.ThrowIfNull(_gameScene, nameof(_gameScene));
        _scene.SwitchTo(_gameScene);
        Play();
    }

    /// <summary>
    /// Handles Mods button activation.
    /// </summary>
    private void OnModsPressed()
    {
        _scene.SwitchToModLoader();
        Mods();
    }

    /// <summary>
    /// Handles Options button activation.
    /// </summary>
    private void OnOptionsPressed()
    {
        _scene.SwitchToOptions();
        Options();
    }

    /// <summary>
    /// Handles Credits button activation.
    /// </summary>
    private void OnCreditsPressed()
    {
        _scene.SwitchToCredits();
        Credits();
    }

    /// <summary>
    /// Handles Quit button activation.
    /// </summary>
    private void OnQuitPressed()
    {
        _ = ExitGameAsync();
    }

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

    /// <summary>
    /// Restores focus to Play when this scene becomes active.
    /// </summary>
    private void OnPostSceneChanged()
    {
        FocusOnPlayBtn();
    }
}
