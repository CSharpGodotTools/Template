using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PacketGen.Tests;

internal sealed class GeneratedAssemblyCompiler
{
    public static Assembly Compile(GeneratorTestRunResult result, IGeneratedFileStore fileStore, string? extraSource = null)
    {
        SyntaxTree genTree = CSharpSyntaxTree.ParseText(result.GeneratedSource);
        SyntaxTree sourceTree = CSharpSyntaxTree.ParseText(result.TestSource);
        SyntaxTree packetStubs = CSharpSyntaxTree.ParseText(MainProjectSource.PacketStubs);

        List<SyntaxTree> syntaxTrees = [sourceTree, genTree, packetStubs];

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
