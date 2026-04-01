using System.Reflection;

namespace PacketGen.Tests;

/// <summary>
/// Reflection helpers used by PacketGen tests.
/// </summary>
public static class ReflectionHelpers
{
    /// <summary>
    /// Reads dictionary count from a public static registry field.
    /// </summary>
    /// <param name="registryType">Registry type containing the dictionary field.</param>
    /// <param name="fieldName">Dictionary field name.</param>
    /// <returns>Dictionary element count.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when field or dictionary count property cannot be resolved.
    /// </exception>
    public static int GetDictionaryCount(Type registryType, string fieldName)
    {
        FieldInfo field = registryType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Unable to find field {fieldName} on {registryType.FullName}.");

        object? dictionary = field.GetValue(null);
        PropertyInfo countProperty = dictionary?.GetType().GetProperty("Count")
            ?? throw new InvalidOperationException($"Unable to read Count from dictionary {fieldName}.");

        return (int)countProperty.GetValue(dictionary)!;
    }
}
