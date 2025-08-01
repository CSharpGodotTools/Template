using Godot;
using System;

namespace __TEMPLATE__.UI;

[Service]
[SceneTree]
public partial class PopupMenu : Control
{
    public event Action OnOpened;
    public event Action OnClosed;
    public event Action OnMainMenuBtnPressed;

    public WorldEnvironment WorldEnvironment { get; private set; }
    public Options Options { get; private set; }

    private VBoxContainer _vbox;
    private PanelContainer _menu;
    private UIConsole _console;

    public override void _Ready()
    {
        TryFindWorldEnvironmentNode();

        _console = GetNode<UIConsole>(AutoloadPaths.Console);
        _menu = Menu;
        _vbox = Navigation;

        Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.UICancel))
        {
            if (UIConsole.Instance.Visible)
            {
                _console.ToggleVisibility();
                return;
            }

            if (Options != null)
            {
                Options.QueueFree();
                Options = null;
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
        Node node = GetTree().Root.FindChild("WorldEnvironment", recursive: true, owned: false);

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
        Options = Options.Instantiate();
        AddChild(Options);
        _menu.Hide();
    }

    private void _OnMainMenuPressed()
    {
        OnMainMenuBtnPressed?.Invoke();
        GetTree().Paused = false;
        Game.SwitchScene(this, Scene.MainMenu);
    }

    private async void _OnQuitPressed()
    {
        await GetNode<Global>(AutoloadPaths.Global).QuitAndCleanup();
    }
}
