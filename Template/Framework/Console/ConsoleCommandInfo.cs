using System;

namespace Framework.Ui.Console;

public class ConsoleCommandInfo
{
    public required string Name { get; set; }
    public required Action<string[]> Code { get; set; }
    public string[] Aliases { get; set; } = [];
}
