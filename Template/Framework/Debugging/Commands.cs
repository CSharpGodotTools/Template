using __TEMPLATE__.Ui.Console;
using GodotUtils;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

public class Commands
{
    public static void RegisterAll()
    {
        GameConsole console = Game.GameConsole;
        console.RegisterCommand("help", CommandHelp);
        console.RegisterCommand("quit", CommandQuit).WithAliases("exit");
        console.RegisterCommand("debug", CommandDebug);
    }

    private static void CommandHelp(string[] args)
    {
        IEnumerable<string> cmds = Game.GameConsole.GetCommands().Select(x => x.Name);
        Game.Logger.Log(cmds.ToFormattedString()!);
    }

    private async static void CommandQuit(string[] args)
    {
        await Game.Application.ExitGameAsync();
    }

    private static void CommandDebug(string[] args)
    {
        if (args.Length <= 0)
        {
            Game.Logger.Log("Specify at least one argument");
            return;
        }

        Game.Logger.Log(args[0]);
    }
}
