using __TEMPLATE__.UI;
using Godot;
using GodotUtils.UI.Console;
using System;

// This was intentionally set to GodotUtils instead of __TEMPLATE__ as GodotUtils relies on MainMenuBtnPressed
// and GodotUtils should NOT have any trace of using __TEMPLATE__.
namespace GodotUtils.UI; 

[SceneTree]
public partial class PopupMenu : Control
{
    public event Action Opened;
    public event Action Closed;
    public event Action MainMenuBtnPressed;

    public WorldEnvironment WorldEnvironment { get; private set; }

    private PanelContainer _menu;
    private VBoxContainer _vbox;
    private Options _options;

    public override void _Ready()
    {
        _menu = Menu;
        _vbox = Navigation;

        WorldEnvironment = TryFindWorldEnvironment(GetTree().Root);

        Services.Register(this);
        CreateOptions();
        HideOptions();
        Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.UICancel))
        {
            if (GameConsole.Visible)
            {
                GameConsole.ToggleVisibility();
                return;
            }

            if (_options.Visible)
            {
                HideOptions();
                ShowPopupMenu();
            }
            else
            {
                ToggleGamePause();
            }
        }
    }

    private void _OnResumePressed()
    {
        Hide();
        GetTree().Paused = false;
    }

    private void _OnOptionsPressed()
    {
        ShowOptions();
        HidePopupMenu();
    }

    private void _OnMainMenuPressed()
    {
        MainMenuBtnPressed?.Invoke();
        GetTree().Paused = false;
        SceneManager.SwitchScene(Scene.MainMenu);
    }

    private async void _OnQuitPressed()
    {
        await Autoloads.Instance.QuitAndCleanup();
    }

    private void CreateOptions()
    {
        _options = Options.Instantiate();
        AddChild(_options);
    }

    private void ShowOptions()
    {
        _options.ProcessMode = ProcessModeEnum.Always;
        _options.Show();
    }

    private void HideOptions()
    {
        _options.ProcessMode = ProcessModeEnum.Disabled;
        _options.Hide();
    }

    private void ShowPopupMenu()
    {
        _menu.Show();
    }

    private void HidePopupMenu()
    {
        _menu.Hide();
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
    }

    private void ResumeGame()
    {
        Visible = false;
        GetTree().Paused = false;
        Closed?.Invoke();
    }

    private static WorldEnvironment TryFindWorldEnvironment(Window root)
    {
        Node node = root.FindChild("WorldEnvironment", recursive: true, owned: false);
        return node as WorldEnvironment;
    }
}
