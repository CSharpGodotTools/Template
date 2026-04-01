namespace PacketGen.Tests;

/// <summary>
/// Represents a property/value pair used in test setup.
/// </summary>
/// <param name="name">Property name.</param>
/// <param name="value">Property value.</param>
internal sealed class PropertyCase(string name, object? value)
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the property value.
    /// </summary>
    public object? Value { get; } = value;
}
