using Microsoft.CodeAnalysis;

namespace PacketGen.Utilities;

/// <summary>
/// Maps type symbols to their corresponding PacketReader Read method suffixes.
/// See __TEMPLATE__.Netcode.PacketReader for all read methods.
/// </summary>
internal static class ReadMethodSuffix
{
    /// <summary>
    /// Returns the suffix for the appropriate Read method defined in PacketReader.cs.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to get the Read method suffix for.</param>
    /// <returns>The method suffix (e.g., "Int", "String", "Bool"), or null if not supported.</returns>
    public static string? Get(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString() switch
        {
            "byte[]" => "Bytes",
            "Godot.Vector2" => "Vector2",
            "Godot.Vector3" => "Vector3",
            "System.Numerics.Vector2" => "Vector2Numerics",

            _ => typeSymbol.SpecialType switch
            {
                SpecialType.System_Byte => "Byte",
                SpecialType.System_SByte => "SByte",
                SpecialType.System_Char => "Char",
                SpecialType.System_String => "String",
                SpecialType.System_Boolean => "Bool",
                SpecialType.System_Int16 => "Short",
                SpecialType.System_UInt16 => "UShort",
                SpecialType.System_Int32 => "Int",
                SpecialType.System_UInt32 => "UInt",
                SpecialType.System_Single => "Float",
                SpecialType.System_Double => "Double",
                SpecialType.System_Decimal => "Decimal",
                SpecialType.System_Int64 => "Long",
                SpecialType.System_UInt64 => "ULong",

                _ => null
            },
        };
    }
}
