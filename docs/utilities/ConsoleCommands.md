> [!NOTE]
> The in-game console can be brought up with `F12`

Registering new commands is easy.
```cs
public override void _Ready()
{
    Game.Console.RegisterCommand("help",  CommandHelp);
    Game.Console.RegisterCommand("quit",  CommandQuit).WithAliases("exit");
    Game.Console.RegisterCommand("debug", CommandDebug);
}

private void CommandHelp(string[] args)
{
    IEnumerable<string> cmds = Game.Console.GetCommands().Select(x => x.Name);
    Logger.Log(cmds.ToFormattedString());
}

private async void CommandQuit(string[] args)
{
    await Global.Instance.ExitGame();
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
```

<img width="344" height="330" alt="image" src="https://github.com/user-attachments/assets/d5ccf33f-316a-44ca-9950-8898a6ee14e3" />

