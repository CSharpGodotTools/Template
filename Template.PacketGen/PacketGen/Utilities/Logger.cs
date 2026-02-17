using Microsoft.CodeAnalysis;
using System.ComponentModel;

namespace PacketGen;

internal static class Logger
{
    private const string Category = "PacketGen";
    private const string InfoId = "PKT0001";
    private const string WarningId = "PKT0002";
    private const string ErrorId = "PKT0003";

    private static readonly DiagnosticDescriptor _infoDescriptor = new(
        InfoId,
        "PacketGen Info",
        "{0}",
        Category,
        DiagnosticSeverity.Info,
        true);

    private static readonly DiagnosticDescriptor _warningDescriptor = new(
        WarningId,
        "PacketGen Warning",
        "{0}",
        Category,
        DiagnosticSeverity.Warning,
        true);

    private static readonly DiagnosticDescriptor _errorDescriptor = new(
        ErrorId,
        "PacketGen Error",
        "{0}",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static SourceProductionContext _context;
    private static bool _initialized;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Init(SourceProductionContext context)
    {
        _context = context;
        _initialized = true;
    }

    public static void Err(ISymbol symbol, string? message)
    {
        Log(symbol, message, _errorDescriptor);
    }

    public static void Warn(ISymbol symbol, string? message)
    {
        Log(symbol, message, _warningDescriptor);
    }

    public static void Info(string? message)
    {
        Info(null, message);
    }

    public static void Info(ISymbol? symbol, string? message)
    {
        Log(symbol, message, _infoDescriptor);
    }

    private static void Log(ISymbol? symbol, string? message, DiagnosticDescriptor descriptor)
    {
        if (!_initialized)
            return;

        string detail = string.IsNullOrWhiteSpace(message) ? "(no details provided)" : message!;

        Location location = Location.None;
        if (symbol != null)
        {
            var locations = symbol.Locations;
            location = locations.Length > 0 ? locations[0] : Location.None;
        }

        Diagnostic diagnostic = Diagnostic.Create(descriptor, location, detail);
        _context.ReportDiagnostic(diagnostic);
    }
}
