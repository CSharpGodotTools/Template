using System;

namespace __TEMPLATE__.Ui.Console;

/// <summary>
/// Describes a console command with its executable delegate and optional aliases.
/// </summary>
public class ConsoleCommandInfo
{
    /// <summary>
    /// Gets or sets the command name used for lookup.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the delegate executed when the command is invoked.
    /// </summary>
    public required Action<string[]> Code { get; set; }

    /// <summary>
    /// Gets or sets alternative names accepted for this command.
    /// </summary>
    public string[] Aliases { get; set; } = [];
}
