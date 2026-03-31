using __TEMPLATE__.Ui.Console;
using GodotUtils;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

public class Commands
{
    public static void RegisterAll(GameConsole console, ILoggerService logger, IApplicationLifetime applicationLifetime)
    {
        console.RegisterCommand("help", _ => CommandHelp(console, logger));
        console.RegisterCommand("quit", _ => CommandQuit(applicationLifetime)).WithAliases("exit");
        console.RegisterCommand("debug", args => CommandDebug(args, logger));
    }

    private static void CommandHelp(GameConsole console, ILoggerService logger)
    {
        IEnumerable<string> cmds = console.GetCommands().Select(x => x.Name);
        logger.Log(cmds.ToFormattedString()!);
    }

    private static async void CommandQuit(IApplicationLifetime applicationLifetime)
    {
        await applicationLifetime.ExitGameAsync();
    }

    private static void CommandDebug(string[] args, ILoggerService logger)
    {
        if (args.Length <= 0)
        {
            logger.Log("Specify at least one argument");
            return;
        }

        logger.Log(args[0]);
    }
}
