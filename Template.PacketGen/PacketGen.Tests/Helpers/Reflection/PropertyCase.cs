namespace PacketGen.Tests;

internal sealed class PropertyCase(string name, object? value)
{
    public string Name { get; } = name;
    public object? Value { get; } = value;
}
