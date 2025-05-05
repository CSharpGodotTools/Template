using Godot;
using System;

namespace Template.UI;

[Service]
[SceneTree]
public partial class UIPopupMenu : Control
{
    public event Action OnOpened;
    public event Action OnClosed;
    public event Action OnMainMenuBtnPressed;

    public WorldEnvironment WorldEnvironment { get; private set; }

    private VBoxContainer _vbox;
    private PanelContainer _menu;
    public Options Options;

    public override void _Ready()
    {
        TryFindWorldEnvironmentNode();

        _menu = Menu;
        _vbox = Navigation;

        Options = Options.Instantiate();
        AddChild(Options);
        Options.Hide();
        Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.UICancel))
        {
            if (UIConsole.Instance.Visible)
            {
                UIConsole.ToggleVisibility();
                return;
            }

            if (Options.Visible)
            {
                Options.Hide();
                _menu.Show();
            }
            else
            {
                Visible = !Visible;
                GetTree().Paused = Visible;

                if (Visible)
                {
                    OnOpened?.Invoke();
                }
                else
                {
                    OnClosed?.Invoke();
                }
            }
        }
    }

    private void TryFindWorldEnvironmentNode()
    {
        Node node = GetTree().Root.FindChild("WorldEnvironment", 
            recursive: true, owned: false);

        if (node is not null and WorldEnvironment worldEnvironment)
        {
            WorldEnvironment = worldEnvironment;
        }
    }

    private void _on_resume_pressed()
    {
        Hide();
        GetTree().Paused = false;
    }

    private void _on_options_pressed()
    {
        Options.Show();
        _menu.Hide();
    }

    private void _on_main_menu_pressed()
    {
        OnMainMenuBtnPressed?.Invoke();
        GetTree().Paused = false;
        Game.SwitchScene(Scene.MainMenu);
    }

    private async void _on_quit_pressed()
    {
        await Global.QuitAndCleanup();
    }
}

