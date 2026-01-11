using __TEMPLATE__.UI.Console;
using Godot;
using GodotUtils;
using System;

// This was intentionally set to GodotUtils instead of __TEMPLATE__ as GodotUtils relies on MainMenuBtnPressed
// and GodotUtils should NOT have any trace of using __TEMPLATE__.
namespace __TEMPLATE__.UI; 

public partial class PopupMenu : Control
{
    #region Exports
    [Export] private PackedScene _optionsPrefab;
    #endregion

    #region Events
    public event Action Opened;
    public event Action Closed;
    public event Action OptionsOpened;
    public event Action OptionsClosed;
    public event Action MainMenuBtnPressed;
    #endregion

    #region Nodes
    private Button _resumeBtn;
    private Button _restartBtn;
    private Button _optionsBtn;
    private Button _mainMenuBtn;
    private Button _quitBtn;

    private VBoxContainer _nav;
    private GameConsole _console;
    private Options _options;
    private Control _menu;
    #endregion   

    #region Godot Overrides
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        InitializeNodes();
        RegisterNodeEvents();
        RegisterGlobalHandlers();

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
        UnregisterGlobalHandlers();
    }
    #endregion

    #region Initialization
    private void InitializeNodes()
    {
        _console = Game.Console;
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

    private void RegisterGlobalHandlers()
    {
        Game.Services.Register(this);
        Game.FocusOutline.RegisterPopupMenu(this);
    }

    private void UnregisterGlobalHandlers()
    {
        Game.FocusOutline.UnregisterPopupMenu(this);
    }
    #endregion

    #region Popup Menu
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
    }

    private void FocusResumeBtn() => _resumeBtn.GrabFocus();
    private void ShowPopupMenu() => _menu.Show();
    private void HidePopupMenu() => _menu.Hide();
    #endregion

    #region Subscribers
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
    #endregion
}
