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

    public static Logger Logger { get; private set; } = new();

    private static Global _instance;
    private OptionsManager _optionsManager;

    public override void _Ready()
    {
        _instance = this;
        _optionsManager = GetNode<OptionsManager>(Autoloads.OptionsManager);
        
        Logger.MessageLogged += UIConsole.AddMessage;

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
        _instance.GetTree().AutoAcceptQuit = false;

        // Handle cleanup here
        OptionsManager.SaveOptions();
        OptionsManager.SaveHotkeys();

        if (OnQuit != null)
        {
            await OnQuit?.Invoke();
        }

        // This must be here because buttons call Global::Quit()
        _instance.GetTree().Quit();
    }
}

