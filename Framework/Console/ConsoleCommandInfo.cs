using System;

namespace GodotUtils.UI.Console;

public class ConsoleCommandInfo
{
    public required string Name { get; set; }
    public required Action<string[]> Code { get; set; } // string[] is the the functions params
    public string[] Aliases { get; set; }
}
