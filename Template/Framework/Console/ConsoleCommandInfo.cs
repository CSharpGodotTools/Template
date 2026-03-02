using System;

namespace Framework.UI.Console;

public class ConsoleCommandInfo
{
    public required string Name { get; set; }
    public required Action<string[]> Code { get; set; }
    public string[] Aliases { get; set; } = [];
}
