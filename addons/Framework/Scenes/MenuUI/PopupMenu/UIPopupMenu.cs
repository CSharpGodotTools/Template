using Godot;
using System;

namespace __TEMPLATE__.UI;

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

    private void _OnResumePressed()
    {
        Hide();
        GetTree().Paused = false;
    }

    private void _OnOptionsPressed()
    {
        Options.Show();
        _menu.Hide();
    }

    private void _OnMainMenuPressed()
    {
        OnMainMenuBtnPressed?.Invoke();
        GetTree().Paused = false;
        Game.SwitchScene(Scene.MainMenu);
    }

#pragma warning disable CA1822 // Mark members as static
    private async void _OnQuitPressed()
#pragma warning restore CA1822 // Mark members as static
    {
        await Global.QuitAndCleanup();
    }
}

