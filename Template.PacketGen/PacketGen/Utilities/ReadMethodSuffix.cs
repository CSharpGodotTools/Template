using Microsoft.CodeAnalysis;

namespace PacketGen.Utilities;

// See Framework.Netcode.PacketReader for all read methods
internal class ReadMethodSuffix
{
    /// <summary>
    /// Returns the suffix for the appropriate Read() method defined in <see href="https://github.com/CSharpGodotTools/Framework/blob/7856ac9ebd0bd3fbda74d991c639c96797c2ced2/Netcode/Packet/PacketReader.cs">PacketReader.cs</see>.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to get the type name from.</param>
    public static string? Get(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString() switch
        {
            "byte[]" => "Bytes",
            "Godot.Vector2" => "Vector2",
            "Godot.Vector3" => "Vector3",

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
