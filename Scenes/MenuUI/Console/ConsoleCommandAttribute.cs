using System;

namespace Template;

[AttributeUsage(AttributeTargets.Method)]
public class ConsoleCommandAttribute(string name, params string[] aliases) : Attribute
{
    public string Name { get; set; } = name;
    public string Description { get; set; }
    public string[] Aliases { get; set; } = aliases;
}

