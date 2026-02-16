using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PacketGen.Tests;

internal sealed class GeneratorTestRunner<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    private static readonly Dictionary<string, string> _trustedPlatformAssemblyLookup = BuildTrustedPlatformAssemblyLookup();

    public static GeneratorTestRunResult? Run(GeneratorTestOptions options)
    {
        HashSet<string> references = new(options.References, StringComparer.OrdinalIgnoreCase);

        foreach (string assemblyName in options.TrustedPlatformAssemblyNames)
        {
            if (_trustedPlatformAssemblyLookup.TryGetValue(assemblyName, out string? assemblyPath))
                references.Add(assemblyPath);
        }

        IEnumerable<SyntaxTree> syntaxTrees = options.Sources.Select(s => CSharpSyntaxTree.ParseText(s));

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: references.Select(r => MetadataReference.CreateFromFile(r)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        IIncrementalGenerator generator = new TGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);

        GeneratorDriverRunResult driverResult = driver.GetRunResult();

        if (driverResult.Results.Length == 0)
            return null;

        Microsoft.CodeAnalysis.GeneratorRunResult runResult = driverResult.Results[0];

        ImmutableArray<GeneratedSourceResult> generatedSources = runResult.GeneratedSources;

        if (generatedSources.Length == 0)
            return null;

        GeneratedSourceResult sourceResult = generatedSources[0];
        string generatedSource = sourceResult.SourceText.ToString();

        string testSource = options.Sources.Length > 0 ? options.Sources[0] : string.Empty;

        return new GeneratorTestRunResult(
            generatedSource,
            options.GeneratedFile,
            [.. references],
            testSource,
            runResult.Diagnostics
        );
    }

    private static Dictionary<string, string> BuildTrustedPlatformAssemblyLookup()
    {
        string? tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;

        if (string.IsNullOrWhiteSpace(tpa))
            return [];

        return tpa.Split(Path.PathSeparator)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key!, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }
}
