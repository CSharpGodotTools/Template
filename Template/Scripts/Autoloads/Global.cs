using Godot;
using System.Threading.Tasks;
using System;
using Template.UI;

namespace Template;

public partial class Global : Node
{
    /// <summary>
    /// If no await calls are needed, add "return await Task.FromResult(1);"
    /// </summary>
    public static event Func<Task> OnQuit;

    [Export] private OptionsManager _optionsManager;

    public static Logger Logger { get; private set; } = new();

    private static Global _instance;

    public override void _Ready()
    {
        _instance = this;
        
        Logger.MessageLogged += UIConsole.AddMessage;

        ModLoaderUI.LoadMods(this);
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

    public static async Task QuitAndCleanup()
    {
        _instance.GetTree().AutoAcceptQuit = false;

        // Handle cleanup here
        _instance._optionsManager.SaveOptions();
        _instance._optionsManager.SaveHotkeys();

        if (OnQuit != null)
        {
            await OnQuit?.Invoke();
        }

        // This must be here because buttons call Global::Quit()
        _instance.GetTree().Quit();
    }
}

