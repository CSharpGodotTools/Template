using Microsoft.CodeAnalysis;

namespace PacketGen.Utilities;

/// <summary>Shared helpers for inspecting Roslyn type symbols across emitters and type handlers.</summary>
internal static class TypeSymbolHelper
{
    private const string ListFullName = "global::System.Collections.Generic.List<T>";
    private const string DictionaryFullName = "global::System.Collections.Generic.Dictionary<TKey, TValue>";

    /// <summary>
    /// Returns <c>true</c> if <paramref name="type"/> is an array, <c>List&lt;T&gt;</c>, or
    /// <c>Dictionary&lt;TKey, TValue&gt;</c> — i.e. a collection that requires deep equality/hashing.
    /// </summary>
    public static bool IsCollectionType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol)
            return true;

        if (type is INamedTypeSymbol named && named.IsGenericType)
            return IsList(named) || IsDictionary(named);

        return false;
    }

    /// <summary>Returns <c>true</c> if <paramref name="type"/> is <c>System.Collections.Generic.List&lt;T&gt;</c>.</summary>
    public static bool IsList(INamedTypeSymbol type) =>
        type.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ListFullName;

    /// <summary>Returns <c>true</c> if <paramref name="type"/> is <c>System.Collections.Generic.Dictionary&lt;TKey, TValue&gt;</c>.</summary>
    public static bool IsDictionary(INamedTypeSymbol type) =>
        type.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == DictionaryFullName;

    /// <summary>Returns the fully-qualified type name with the <c>global::</c> alias prefix removed.</summary>
    public static string ToTypeName(ITypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty);
}
