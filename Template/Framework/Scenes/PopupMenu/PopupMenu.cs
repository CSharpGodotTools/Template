using Framework.UI.Console;
using Godot;
using GodotUtils;
using System;

// This was intentionally set to GodotUtils instead of __TEMPLATE__ as GodotUtils relies on MainMenuBtnPressed
// and GodotUtils should NOT have any trace of using Framework.
namespace Framework.UI; 

public partial class PopupMenu : Control
{
    // Exports
    [Export] private PackedScene _optionsPrefab;

    // Events
    public event Action Opened;
    public event Action Closed;
    public event Action OptionsOpened;
    public event Action OptionsClosed;
    public event Action MainMenuBtnPressed;

    // Nodes
    private Button _resumeBtn;
    private Button _restartBtn;
    private Button _optionsBtn;
    private Button _mainMenuBtn;
    private Button _quitBtn;

    private VBoxContainer _nav;
    private GameConsole _console;
    private Options _options;
    private Control _menu;

    // Godot Overrides
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        GameFramework.Services.Register(this);
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
        _console = GameFramework.Console;
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
        GameFramework.FocusOutline.ClearFocus();
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
        GameFramework.FocusOutline.ClearFocus();
    }

    private void FocusResumeBtn() => _resumeBtn.GrabFocus();
    private void ShowPopupMenu() => _menu.Show();
    private void HidePopupMenu() => _menu.Hide();

    // Subscribers
    private void OnResumePressed()
    {
        Hide();
        GetTree().Paused = false;
        Closed?.Invoke();
        GameFramework.FocusOutline.ClearFocus();
    }

    private void OnRestartPressed()
    {
        GetTree().Paused = false;
        GameFramework.Scene.ResetCurrentScene();
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
        GameFramework.Scene.SwitchToMainMenu();
    }

    private void OnQuitPressed()
    {
        TaskUtils.FireAndForget(Autoloads.Instance.ExitGame);
    }
}
