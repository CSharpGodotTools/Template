using GodotUtils;
using GodotUtils.UI.Console;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.UI;

public class Commands
{
    public static void RegisterAll()
    {
        GameConsole.RegisterCommand("help",  CommandHelp);
        GameConsole.RegisterCommand("quit",  CommandQuit).WithAliases("exit");
        GameConsole.RegisterCommand("debug", CommandDebug);
    }

    private static void CommandHelp(string[] args)
    {
        IEnumerable<string> cmds = GameConsole.Instance.GetCommands().Select(x => x.Name);
        Logger.Log(cmds.ToFormattedString());
    }

    private async static void CommandQuit(string[] args)
    {
        await Autoloads.Instance.QuitAndCleanup();
    }

    private static void CommandDebug(string[] args)
    {
        if (args.Length <= 0)
        {
            Logger.Log("Specify at least one argument");
            return;
        }

        Logger.Log(args[0]);
    }
}
