using System.Reflection;

namespace PacketGen.Tests;

public static class ReflectionHelpers
{
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
