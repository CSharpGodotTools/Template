using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Immutable;
using System.Reflection;

namespace PacketGen.Tests;

/// <summary>
/// Compiles generated source and test stubs into an in-memory assembly for assertions.
/// </summary>
internal sealed class GeneratedAssemblyCompiler
{
    /// <summary>
    /// Compiles generated source output into an in-memory test assembly.
    /// </summary>
    /// <param name="result">Generator output payload.</param>
    /// <param name="fileStore">File store for diagnostics artifacts.</param>
    /// <param name="extraSource">Optional extra source to include.</param>
    /// <returns>Loaded in-memory assembly.</returns>
    /// <exception cref="AssertionException">
    /// Thrown when compilation fails; diagnostics are written to file store.
    /// </exception>
    public static Assembly Compile(GeneratorTestRunResult result, IGeneratedFileStore fileStore, string? extraSource = null)
    {
        SyntaxTree genTree = CSharpSyntaxTree.ParseText(result.GeneratedSource);
        SyntaxTree sourceTree = CSharpSyntaxTree.ParseText(result.TestSource);
        SyntaxTree packetStubs = CSharpSyntaxTree.ParseText(MainProjectSource.PacketStubs);

        List<SyntaxTree> syntaxTrees = [sourceTree, genTree, packetStubs];

        // Include optional source only when a non-empty snippet is provided.
        if (!string.IsNullOrWhiteSpace(extraSource))
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(extraSource));

        List<PortableExecutableReference> references = [.. result.References.Select(r => MetadataReference.CreateFromFile(r))];

        CSharpCompilation genCompilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly_Generated",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using MemoryStream ms = new();
        EmitResult emit = genCompilation.Emit(ms);

        ImmutableArray<Diagnostic> diagnostics = [.. emit.Diagnostics.Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)];

        // Persist compilation diagnostics and fail the test when emit was unsuccessful.
        if (!emit.Success)
        {
            string contents = CompilationDiagnosticsFormatter.Format(diagnostics, references, result.GeneratedSource);

            fileStore.WriteErrors($"{result.GeneratedFile}_Errors.txt", contents);
            Assert.That(false, $"Test assembly failed with errors, see {result.GeneratedFile}_Errors.txt");
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
}
