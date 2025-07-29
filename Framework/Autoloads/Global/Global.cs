using Godot;
using System.Threading.Tasks;
using System;
using __TEMPLATE__.UI;

namespace __TEMPLATE__;

public partial class Global : Node
{
    public event Func<Task> PreQuit;

    public static Global Instance { get; private set; }

    public Logger Logger { get; private set; } = new();

    public override void _Ready()
    {
        Instance = this;
        
        Logger.MessageLogged += GetNode<UIConsole>(Autoloads.Console).AddMessage;

        //ModLoaderUI.LoadMods(this);
    }

    public override void _PhysicsProcess(double delta)
    {
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
        GetTree().AutoAcceptQuit = false;

        // Wait for cleanup
        if (PreQuit != null)
        {
            await PreQuit?.Invoke();
        }

        // This must be here because buttons call Global::Quit()
        GetTree().Quit();
    }
}
