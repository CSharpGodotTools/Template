using Godot;
using System.Threading.Tasks;
using System;
using __TEMPLATE__.UI;

namespace __TEMPLATE__;

public partial class Global : Node
{
    /// <summary>
    /// If no await calls are needed, add "return await Task.FromResult(1);"
    /// </summary>
    public event Func<Task> OnQuit;

    public static Global Instance { get; private set; }

    public Logger Logger { get; private set; } = new();

    private OptionsManager _optionsManager;

    public override void _Ready()
    {
        Instance = this;

        _optionsManager = GetNode<OptionsManager>(Autoloads.OptionsManager);
        
        Logger.MessageLogged += GetNode<UIConsole>(Autoloads.Console).AddMessage;

        //ModLoaderUI.LoadMods(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.Fullscreen))
        {
            _optionsManager.ToggleFullscreen();
        }

        Logger.Update();
    }

    public override async void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            await QuitAndCleanup();
        }
    }

    public async Task QuitAndCleanup()
    {
        Instance.GetTree().AutoAcceptQuit = false;

        // Handle cleanup here
        OptionsManager optionsManager = GetNode<OptionsManager>(Autoloads.OptionsManager);
        optionsManager.SaveOptions();
        optionsManager.SaveHotkeys();

        if (OnQuit != null)
        {
            await OnQuit?.Invoke();
        }

        // This must be here because buttons call Global::Quit()
        Instance.GetTree().Quit();
    }
}

