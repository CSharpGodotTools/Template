using GodotUtils;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.UI;

public partial class UIConsole
{
    [ConsoleCommand("help")]
    private void Help()
    {
        IEnumerable<string> cmds = Commands.Select(x => x.Name);

        Game.Log(cmds.ToFormattedString());
    }

    [ConsoleCommand("quit", "exit")]
    private async void Quit()
    {
        await GetNode<Global>(AutoloadPaths.Global).QuitAndCleanup();
    }

    [ConsoleCommand("debug")]
    private void Debug(int x)
    {
        Game.Log(x);
    }
}
