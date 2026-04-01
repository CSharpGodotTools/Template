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
    /// <param name="type">Type symbol to inspect.</param>
    /// <returns><see langword="true"/> when the type is a supported collection shape.</returns>
    public static bool IsCollectionType(ITypeSymbol type)
    {
        // Arrays are always treated as deep-compare/deep-hash collections.
        if (type is IArrayTypeSymbol)
            return true;

        // Generic named types may be supported list or dictionary collections.
        if (type is INamedTypeSymbol named && named.IsGenericType)
            return IsList(named) || IsDictionary(named);

        return false;
    }

    /// <summary>Returns <c>true</c> if <paramref name="type"/> is <c>System.Collections.Generic.List&lt;T&gt;</c>.</summary>
    /// <param name="type">Named type symbol to inspect.</param>
    /// <returns><see langword="true"/> when the type is <c>List&lt;T&gt;</c>.</returns>
    public static bool IsList(INamedTypeSymbol type) =>
        type.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ListFullName;

    /// <summary>Returns <c>true</c> if <paramref name="type"/> is <c>System.Collections.Generic.Dictionary&lt;TKey, TValue&gt;</c>.</summary>
    /// <param name="type">Named type symbol to inspect.</param>
    /// <returns><see langword="true"/> when the type is <c>Dictionary&lt;TKey, TValue&gt;</c>.</returns>
    public static bool IsDictionary(INamedTypeSymbol type) =>
        type.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == DictionaryFullName;

    /// <summary>Returns the fully-qualified type name with the <c>global::</c> alias prefix removed.</summary>
    /// <param name="type">Type symbol to format.</param>
    /// <returns>Fully-qualified type name without the <c>global::</c> prefix.</returns>
    public static string ToTypeName(ITypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty);
}
