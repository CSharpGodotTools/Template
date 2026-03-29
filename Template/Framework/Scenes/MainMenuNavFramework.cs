using Godot;
using GodotUtils;
using System;
using System.Threading.Tasks;

namespace __TEMPLATE__.Ui;

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
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(MainMenuNavFramework)} was not configured before _Ready.");

        _viewport = GetViewport();
        _playBtn = GetNode<Button>("Play");

        FocusOnPlayBtn();

        _scene.PostSceneChanged += OnPostSceneChanged;

        _viewport.GuiFocusChanged += OnGuiFocusChanged;
    }

    private void OnGuiFocusChanged(Control node)
    {
        _focusWasNeverChanged = false;
    }

    public override void _Input(InputEvent @event)
    {
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
    private void FocusOnPlayBtn()
    {
        _focusOutline.Focus(_playBtn);
    }

    // Abstract
    protected abstract void Play();
    protected abstract void Mods();
    protected abstract void Options();
    protected abstract void Credits();
    protected abstract void Quit();

    // Subscribers
    private void OnPlayPressed()
    {
        ArgumentNullException.ThrowIfNull(_gameScene, nameof(_gameScene));
        _scene.SwitchTo(_gameScene);
        Play();
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

    private void OnPostSceneChanged()
    {
        FocusOnPlayBtn();
    }
}
