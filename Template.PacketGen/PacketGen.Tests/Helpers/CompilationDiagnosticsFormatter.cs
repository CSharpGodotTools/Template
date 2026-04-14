using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace PacketGen.Tests;

/// <summary>
/// Formats compilation diagnostics and generated source into a readable debug report.
/// </summary>
internal static class CompilationDiagnosticsFormatter
{
    /// <summary>
    /// Formats diagnostics, references, and generated source for failure snapshots.
    /// </summary>
    /// <param name="diagnostics">Diagnostics to format.</param>
    /// <param name="references">Metadata references used for compilation.</param>
    /// <param name="generatedSource">Generated source text.</param>
    /// <returns>Formatted diagnostics report.</returns>
    public static string Format(
        ImmutableArray<Diagnostic> diagnostics,
        IEnumerable<PortableExecutableReference> references,
        string generatedSource)
    {
        var sb = new StringBuilder();

        sb.AppendLine("========= Errors =========\n");

        const int sevWidth = 8;
        const int idWidth = 7;
        const int locWidth = 18;

        static string Pad(string s, int w) => s.Length >= w ? s : s + new string(' ', w - s.Length);

        foreach (Diagnostic d in diagnostics)
        {
            string sev = Pad(d.Severity.ToString(), sevWidth);
            string id = Pad(d.Id, idWidth);

            string loc = d.Location == Location.None
                ? Pad("NoLocation", locWidth)
                : Pad(d.Location.GetLineSpan().ToString(), locWidth);

            string msg = d.GetMessage().Replace("\r\n", " ").Replace("\n", " ");

            sb.AppendLine($"{sev} {id} {loc} : {msg}");
        }

        sb.AppendLine();
        sb.AppendLine("========= References =========\n");

        List<string> referencePaths = [.. references
            .OfType<PortableExecutableReference>()
            .Select(r => r.FilePath ?? string.Empty)
            .Where(p => !string.IsNullOrEmpty(p))];

        foreach (string @ref in referencePaths)
            sb.AppendLine(@ref);

        sb.AppendLine();
        sb.AppendLine("========= Generated source =========\n");
        sb.AppendLine(generatedSource);

        return sb.ToString();
    }
}
