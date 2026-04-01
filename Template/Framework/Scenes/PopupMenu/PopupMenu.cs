using __TEMPLATE__.Ui.Console;
using Godot;
using GodotUtils;
using System;

// This was intentionally set to GodotUtils instead of __TEMPLATE__ as GodotUtils relies on MainMenuBtnPressed
// and GodotUtils should NOT have any trace of using __TEMPLATE__.
namespace __TEMPLATE__.Ui;

/// <summary>
/// In-game pause popup that exposes resume, options, restart, main menu, and quit actions.
/// </summary>
public partial class PopupMenu : Control, ISceneDependencyReceiver
{
    // Exports
    [Export] private PackedScene _optionsPrefab = null!;

    // Events
    /// <summary>
    /// Raised when the popup menu becomes visible.
    /// </summary>
    public event Action? Opened;

    /// <summary>
    /// Raised when the popup menu is dismissed.
    /// </summary>
    public event Action? Closed;

    /// <summary>
    /// Raised when the options panel is opened from the popup menu.
    /// </summary>
    public event Action? OptionsOpened;

    /// <summary>
    /// Raised when the options panel is closed and control returns to the popup menu.
    /// </summary>
    public event Action? OptionsClosed;

    /// <summary>
    /// Raised before switching from gameplay to the main menu.
    /// </summary>
    public event Action? MainMenuBtnPressed;

    // Nodes
    private Button _resumeBtn = null!;
    private Button _restartBtn = null!;
    private Button _optionsBtn = null!;
    private Button _mainMenuBtn = null!;
    private Button _quitBtn = null!;

    private VBoxContainer _nav = null!;
    private Services _services = null!;
    private FocusOutlineManager _focusOutline = null!;
    private SceneManager _sceneManager = null!;
    private IApplicationLifetime _applicationLifetime = null!;
    private IBackgroundTaskTracker _backgroundTasks = null!;
    private AudioManager _audioManager = null!;
    private OptionsManager _optionsManager = null!;
    private GameConsole _console = null!;
    private GameServices _runtimeServices = null!;
    private Options _options = null!;
    private Control _menu = null!;
    private bool _isConfigured;

    /// <summary>
    /// Supplies runtime services required by the popup and marks it ready for initialization.
    /// </summary>
    /// <param name="services">Resolved runtime services for the active game session.</param>
    public void Configure(GameServices services)
    {
        _runtimeServices = services;
        _services = services.ScopedServices;
        _focusOutline = services.FocusOutline;
        _sceneManager = services.SceneManager;
        _applicationLifetime = services.ApplicationLifetime;
        _backgroundTasks = services.BackgroundTasks;
        _audioManager = services.AudioManager;
        _optionsManager = services.OptionsManager;
        _console = services.GameConsole;
        _isConfigured = true;
    }

    public override void _EnterTree()
    {
        SceneComposition.ConfigureNodeFromGame(this);
    }

    // Godot Overrides
    public override void _Ready()
    {
        // Composition must configure runtime dependencies before scene readiness.
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(PopupMenu)} was not configured before _Ready.");

        ProcessMode = ProcessModeEnum.Always;
        InitializeNodes();
        _services.Register(this);
        RegisterNodeEvents();

        CreateOptions();
        HideOptions();
        Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Ignore frames where the cancel action was not pressed.
        if (!Input.IsActionJustPressed(InputActions.UICancel))
            return;

        // Close the console first when it is currently visible.
        if (_console.Visible)
        {
            _console.ToggleVisibility();
            return;
        }

        // Back out of options into the pause menu when options are open.
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
    /// <summary>
    /// Resolves required scene nodes used by popup controls.
    /// </summary>
    private void InitializeNodes()
    {
        _nav = GetNode<VBoxContainer>("%Navigation");
        _menu = GetNode<Control>("Menu");

        _resumeBtn = _nav.GetNode<Button>("Resume");
        _restartBtn = _nav.GetNode<Button>("Restart");
        _optionsBtn = _nav.GetNode<Button>("Options");
        _mainMenuBtn = _nav.GetNode<Button>("Main Menu");
        _quitBtn = _nav.GetNode<Button>("Quit");
    }

    /// <summary>
    /// Subscribes popup button signals to local handlers.
    /// </summary>
    private void RegisterNodeEvents()
    {
        _resumeBtn.Pressed += OnResumePressed;
        _restartBtn.Pressed += OnRestartPressed;
        _optionsBtn.Pressed += OnOptionsPressed;
        _mainMenuBtn.Pressed += OnMainMenuPressed;
        _quitBtn.Pressed += OnQuitPressed;
    }

    /// <summary>
    /// Unsubscribes popup button signals from local handlers.
    /// </summary>
    private void UnregisterNodeEvents()
    {
        _resumeBtn.Pressed -= OnResumePressed;
        _restartBtn.Pressed -= OnRestartPressed;
        _optionsBtn.Pressed -= OnOptionsPressed;
        _mainMenuBtn.Pressed -= OnMainMenuPressed;
        _quitBtn.Pressed -= OnQuitPressed;
    }

    // Popup Menu
    /// <summary>
    /// Instantiates and adds the options panel as a configured child node.
    /// </summary>
    private void CreateOptions()
    {
        _options = SceneComposition.InstantiateAndConfigure<Options>(_optionsPrefab, _runtimeServices);
        AddChild(_options);
    }

    /// <summary>
    /// Shows the options panel and emits <see cref="OptionsOpened"/>.
    /// </summary>
    private void ShowOptions()
    {
        _options.ProcessMode = ProcessModeEnum.Always;
        _options.Show();
        OptionsOpened?.Invoke();
    }

    /// <summary>
    /// Hides the options panel, restores popup focus, and emits <see cref="OptionsClosed"/>.
    /// </summary>
    private void HideOptions()
    {
        _options.ProcessMode = ProcessModeEnum.Disabled;
        _options.Hide();
        OptionsClosed?.Invoke();
        _focusOutline.ClearFocus();
        FocusResumeBtn();
    }

    /// <summary>
    /// Toggles game pause state and popup visibility.
    /// </summary>
    private void ToggleGamePause()
    {
        // Resume when the popup is already shown, otherwise pause gameplay.
        if (Visible)
            ResumeGame();
        else
            PauseGame();
    }

    /// <summary>
    /// Pauses the scene tree and opens the popup menu.
    /// </summary>
    private void PauseGame()
    {
        Visible = true;
        GetTree().Paused = true;
        Opened?.Invoke();
        FocusResumeBtn();
    }

    /// <summary>
    /// Resumes gameplay and closes the popup menu.
    /// </summary>
    private void ResumeGame()
    {
        Visible = false;
        GetTree().Paused = false;
        Closed?.Invoke();
        _focusOutline.ClearFocus();
    }

    /// <summary>
    /// Focuses the resume button for keyboard/controller navigation.
    /// </summary>
    private void FocusResumeBtn() => _resumeBtn.GrabFocus();

    /// <summary>
    /// Shows the popup menu button container.
    /// </summary>
    private void ShowPopupMenu() => _menu.Show();

    /// <summary>
    /// Hides the popup menu button container.
    /// </summary>
    private void HidePopupMenu() => _menu.Hide();

    // Subscribers
    /// <summary>
    /// Handles resume action from the popup menu.
    /// </summary>
    private void OnResumePressed() => ResumeGame();

    /// <summary>
    /// Reloads the current scene and clears pause state.
    /// </summary>
    private void OnRestartPressed()
    {
        GetTree().Paused = false;
        _sceneManager.ResetCurrentScene();
    }

    /// <summary>
    /// Opens options and hides the button list portion of the popup.
    /// </summary>
    private void OnOptionsPressed()
    {
        ShowOptions();
        HidePopupMenu();
    }

    /// <summary>
    /// Triggers main-menu transition callbacks and switches scenes.
    /// </summary>
    private void OnMainMenuPressed()
    {
        MainMenuBtnPressed?.Invoke();
        GetTree().Paused = false;
        _sceneManager.SwitchToMainMenu();
    }

    /// <summary>
    /// Queues asynchronous game exit through the background task tracker.
    /// </summary>
    private void OnQuitPressed()
    {
        _backgroundTasks.Run(_ => _applicationLifetime.ExitGameAsync(), "PopupMenu.ExitGame");
    }
}
