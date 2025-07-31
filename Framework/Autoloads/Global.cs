using Godot;
using System.Threading.Tasks;
using System;

namespace __TEMPLATE__;

public partial class Global : Node
{
    public event Func<Task> PreQuit;

    //public override void _Ready()
    //{
    //    ModLoaderUI.LoadMods(this);
    //}

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
