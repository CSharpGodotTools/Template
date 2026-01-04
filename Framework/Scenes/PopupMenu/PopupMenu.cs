using __TEMPLATE__.UI.Console;
using Godot;
using GodotUtils;
using System;

// This was intentionally set to GodotUtils instead of __TEMPLATE__ as GodotUtils relies on MainMenuBtnPressed
// and GodotUtils should NOT have any trace of using __TEMPLATE__.
namespace __TEMPLATE__.UI; 

public partial class PopupMenu : Control
{
    [Export] private PackedScene _optionsPrefab;
    [Export] private Button _resumeBtn;
    [Export] private Button _restartBtn;
    [Export] private Button _optionsBtn;
    [Export] private Button _mainMenuBtn;
    [Export] private Button _quitBtn;

    public event Action Opened;
    public event Action Closed;
    public event Action OptionsOpened;
    public event Action OptionsClosed;
    public event Action MainMenuBtnPressed;

    private GameConsole _console;
    private PanelContainer _menu;
    private VBoxContainer _nav;
    private Options _options;

    public override void _Ready()
    {
        _console = Game.Console;
        _menu = GetNode<PanelContainer>("%Menu");
        _nav = GetNode<VBoxContainer>("%Navigation");

        _resumeBtn.Pressed += OnResumePressed;
        _restartBtn.Pressed += OnRestartPressed;
        _optionsBtn.Pressed += OnOptionsPressed;
        _mainMenuBtn.Pressed += OnMainMenuPressed;
        _quitBtn.Pressed += OnQuitPressed;

        Game.Services.Register(this);
        Game.FocusOutline.RegisterPopupMenu(this);
        CreateOptions();
        HideOptions();
        Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Input.IsActionJustPressed(InputActions.UICancel))
            return;

        if (_console.Visible)
        {
            _console.ToggleVisibility();
            return;
        }

        if (_options.Visible)
        {
            HideOptions();
            ShowPopupMenu();
            return;
        }

        ToggleGamePause();
    }

    public override void _ExitTree()
    {
        _resumeBtn.Pressed -= OnResumePressed;
        _restartBtn.Pressed -= OnRestartPressed;
        _optionsBtn.Pressed -= OnOptionsPressed;
        _mainMenuBtn.Pressed -= OnMainMenuPressed;
        _quitBtn.Pressed -= OnQuitPressed;

        Game.FocusOutline.UnregisterPopupMenu(this);
    }

    private void OnResumePressed()
    {
        Hide();
        GetTree().Paused = false;
        Closed?.Invoke();
    }

    private void OnRestartPressed()
    {
        GetTree().Paused = false;
        Game.Scene.ResetCurrentScene();
    }

    private void OnOptionsPressed()
    {
        ShowOptions();
        HidePopupMenu();
    }

    private void OnMainMenuPressed()
    {
        MainMenuBtnPressed?.Invoke();
        GetTree().Paused = false;
        Game.Scene.SwitchToMainMenu();
    }

    private void OnQuitPressed()
    {
        BaseAutoloads.Instance.ExitGame().FireAndForget();
    }

    private void CreateOptions()
    {
        _options = _optionsPrefab.Instantiate<Options>();
        AddChild(_options);
    }

    private void ShowOptions()
    {
        _options.ProcessMode = ProcessModeEnum.Always;
        _options.Show();
        OptionsOpened?.Invoke();
    }

    private void HideOptions()
    {
        _options.ProcessMode = ProcessModeEnum.Disabled;
        _options.Hide();
        OptionsClosed?.Invoke();
        FocusResumeBtn();
    }

    private void ShowPopupMenu()
    {
        _menu.Show();
    }

    private void HidePopupMenu() => _menu.Hide();

    private void ToggleGamePause()
    {
        if (Visible)
            ResumeGame();
        else
            PauseGame();
    }

    private void FocusResumeBtn() => GetNode<Button>("%Resume").GrabFocus();

    private void PauseGame()
    {
        Visible = true;
        GetTree().Paused = true;
        Opened?.Invoke();
        FocusResumeBtn();
    }

    private void ResumeGame()
    {
        Visible = false;
        GetTree().Paused = false;
        Closed?.Invoke();
    }
}
