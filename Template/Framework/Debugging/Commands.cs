using __TEMPLATE__.Ui.Console;
using GodotUtils;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Registers built-in console commands for common debugging and application control tasks.
/// </summary>
public class Commands
{
    /// <summary>
    /// Registers all built-in commands on the provided console instance.
    /// </summary>
    /// <param name="console">Console that receives command registrations.</param>
    /// <param name="logger">Logger used for command output.</param>
    /// <param name="applicationLifetime">Application lifetime service used by quit commands.</param>
    public static void RegisterAll(GameConsole console, ILoggerService logger, IApplicationLifetime applicationLifetime)
    {
        console.RegisterCommand("help", _ => CommandHelp(console, logger));
        console.RegisterCommand("quit", _ => CommandQuit(applicationLifetime)).WithAliases("exit");
        console.RegisterCommand("debug", args => CommandDebug(args, logger));
    }

    /// <summary>
    /// Logs the list of registered console command names.
    /// </summary>
    /// <param name="console">Console containing command registrations.</param>
    /// <param name="logger">Logger used for command output.</param>
    private static void CommandHelp(GameConsole console, ILoggerService logger)
    {
        IEnumerable<string> cmds = console.GetCommands().Select(x => x.Name);
        logger.Log(cmds.ToFormattedString()!);
    }

    /// <summary>
    /// Requests asynchronous game shutdown.
    /// </summary>
    /// <param name="applicationLifetime">Application lifetime service.</param>
    private static async void CommandQuit(IApplicationLifetime applicationLifetime)
    {
        await applicationLifetime.ExitGameAsync();
    }

    /// <summary>
    /// Logs the first provided argument for quick debug output.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <param name="logger">Logger used for output.</param>
    private static void CommandDebug(string[] args, ILoggerService logger)
    {
        // Require at least one argument before attempting debug output.
        if (args.Length <= 0)
        {
            logger.Log("Specify at least one argument");
            return;
        }

        logger.Log(args[0]);
    }
}
