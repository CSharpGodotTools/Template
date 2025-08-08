using Godot;
using GodotUtils;
using GodotUtils.UI.Console;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.UI;

public partial class Commands : Node
{
    public override void _Ready()
    {
        GameConsole.RegisterCommand("help",  CommandHelp);
        GameConsole.RegisterCommand("quit",  CommandQuit).WithAliases("exit");
        GameConsole.RegisterCommand("debug", CommandDebug);
    }

    private void CommandHelp(string[] args)
    {
        IEnumerable<string> cmds = GameConsole.Instance.GetCommands().Select(x => x.Name);
        Logger.Log(cmds.ToFormattedString());
    }

    private async void CommandQuit(string[] args)
    {
        await Global.Instance.QuitAndCleanup();
    }

    private void CommandDebug(string[] args)
    {
        if (args.Length <= 0)
        {
            Logger.Log("Specify at least one argument");
            return;
        }

        Logger.Log(args[0]);
    }
}
