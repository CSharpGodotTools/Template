using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.Ui;

public abstract partial class MainMenuNavFramework : Node
{
    // Exports
    [Export] private PackedScene _gameScene = null!;

    // Fields
    private SceneManager _scene = null!;
    private Viewport _viewport = null!;
    private Button _playBtn = null!;
    private bool _focusWasNeverChanged = true;

    // Godot Overrides
    public override void _Ready()
    {
        _scene = Game.Scene;
        _viewport = GetViewport();
        _playBtn = GetNode<Button>("Play");

        FocusOnPlayBtn();

        Game.Scene.PostSceneChanged += OnPostSceneChanged;

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
        Game.Scene.PostSceneChanged -= OnPostSceneChanged;
    }

    // FocusOnPlayBtn
    private void FocusOnPlayBtn()
    {
        Game.FocusOutline.Focus(_playBtn);
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

    private async static void OnQuitPressed()
    {
        await Autoloads.Instance!.ExitGame();
    }

    private void OnPostSceneChanged()
    {
        FocusOnPlayBtn();
    }
}
