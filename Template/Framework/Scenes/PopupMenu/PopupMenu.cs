using __TEMPLATE__.Ui.Console;
using Godot;
using GodotUtils;
using System;

// This was intentionally set to GodotUtils instead of __TEMPLATE__ as GodotUtils relies on MainMenuBtnPressed
// and GodotUtils should NOT have any trace of using __TEMPLATE__.
namespace __TEMPLATE__.Ui;

public partial class PopupMenu : Control
{
    // Exports
    [Export] private PackedScene _optionsPrefab = null!;

    // Events
    public event Action? Opened;
    public event Action? Closed;
    public event Action? OptionsOpened;
    public event Action? OptionsClosed;
    public event Action? MainMenuBtnPressed;

    // Nodes
    private Button _resumeBtn = null!;
    private Button _restartBtn = null!;
    private Button _optionsBtn = null!;
    private Button _mainMenuBtn = null!;
    private Button _quitBtn = null!;

    private VBoxContainer _nav = null!;
    private GameConsole _console = null!;
    private Options _options = null!;
    private Control _menu = null!;

    // Godot Overrides
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Game.ScopedServices.Register(this);
        InitializeNodes();
        RegisterNodeEvents();

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
        UnregisterNodeEvents();
    }

    // Initialization Methods
    private void InitializeNodes()
    {
        _console = Game.GameConsole;
        _nav = GetNode<VBoxContainer>("%Navigation");
        _menu = GetNode<Control>("Menu");

        _resumeBtn = _nav.GetNode<Button>("Resume");
        _restartBtn = _nav.GetNode<Button>("Restart");
        _optionsBtn = _nav.GetNode<Button>("Options");
        _mainMenuBtn = _nav.GetNode<Button>("Main Menu");
        _quitBtn = _nav.GetNode<Button>("Quit");
    }

    private void RegisterNodeEvents()
    {
        _resumeBtn.Pressed += OnResumePressed;
        _restartBtn.Pressed += OnRestartPressed;
        _optionsBtn.Pressed += OnOptionsPressed;
        _mainMenuBtn.Pressed += OnMainMenuPressed;
        _quitBtn.Pressed += OnQuitPressed;
    }

    private void UnregisterNodeEvents()
    {
        _resumeBtn.Pressed -= OnResumePressed;
        _restartBtn.Pressed -= OnRestartPressed;
        _optionsBtn.Pressed -= OnOptionsPressed;
        _mainMenuBtn.Pressed -= OnMainMenuPressed;
        _quitBtn.Pressed -= OnQuitPressed;
    }

    // Popup Menu
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
        Game.FocusOutline.ClearFocus();
        FocusResumeBtn();
    }

    private void ToggleGamePause()
    {
        if (Visible)
            ResumeGame();
        else
            PauseGame();
    }

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
        Game.FocusOutline.ClearFocus();
    }

    private void FocusResumeBtn() => _resumeBtn.GrabFocus();
    private void ShowPopupMenu() => _menu.Show();
    private void HidePopupMenu() => _menu.Hide();

    // Subscribers
    private void OnResumePressed() => ResumeGame();

    private void OnRestartPressed()
    {
        GetTree().Paused = false;
        Game.SceneManager.ResetCurrentScene();
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
        Game.SceneManager.SwitchToMainMenu();
    }

    private void OnQuitPressed()
    {
        TaskUtils.FireAndForget(Game.Application.ExitGameAsync);
    }
}
