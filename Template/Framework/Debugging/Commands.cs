using Framework.UI.Console;
using GodotUtils;
using System.Collections.Generic;
using System.Linq;

namespace Framework.UI;

public class Commands
{
    public static void RegisterAll()
    {
        GameConsole console = GameFramework.Console;
        console.RegisterCommand("help", CommandHelp);
        console.RegisterCommand("quit", CommandQuit).WithAliases("exit");
        console.RegisterCommand("debug", CommandDebug);
    }

    private static void CommandHelp(string[] args)
    {
        IEnumerable<string> cmds = GameFramework.Console.GetCommands().Select(x => x.Name);
        GameFramework.Logger.Log(cmds.ToFormattedString());
    }

    private async static void CommandQuit(string[] args)
    {
        await Autoloads.Instance.ExitGame();
    }

    private static void CommandDebug(string[] args)
    {
        if (args.Length <= 0)
        {
            GameFramework.Logger.Log("Specify at least one argument");
            return;
        }

        GameFramework.Logger.Log(args[0]);
    }
}
