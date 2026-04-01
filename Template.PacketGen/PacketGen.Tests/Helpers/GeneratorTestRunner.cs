using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace PacketGen.Tests;

/// <summary>
/// Runs incremental generator tests and returns generated-source snapshots.
/// </summary>
/// <typeparam name="TGenerator">Generator type under test.</typeparam>
internal sealed class GeneratorTestRunner<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    private static readonly Dictionary<string, string> _trustedPlatformAssemblyLookup = BuildTrustedPlatformAssemblyLookup();

    /// <summary>
    /// Executes generator run for provided options and returns first generated result.
    /// </summary>
    /// <param name="options">Generator run options.</param>
    /// <returns>Run result containing generated source and diagnostics, or null when none produced.</returns>
    public static GeneratorTestRunResult? Run(GeneratorTestOptions options)
    {
        HashSet<string> references = new(options.References, StringComparer.OrdinalIgnoreCase);

        foreach (string assemblyName in options.TrustedPlatformAssemblyNames)
        {
            // Add trusted platform references only when they resolve to a known path.
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

        // Return null when the generator produced no run results.
        if (driverResult.Results.Length == 0)
            return null;

        Microsoft.CodeAnalysis.GeneratorRunResult runResult = driverResult.Results[0];

        ImmutableArray<GeneratedSourceResult> generatedSources = runResult.GeneratedSources;

        // Return null when no source files were generated.
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

    /// <summary>
    /// Builds a lookup from trusted platform assembly names to file paths.
    /// </summary>
    /// <returns>Assembly-name lookup dictionary.</returns>
    private static Dictionary<string, string> BuildTrustedPlatformAssemblyLookup()
    {
        string? tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;

        // Return an empty lookup when runtime assembly metadata is unavailable.
        if (string.IsNullOrWhiteSpace(tpa))
            return [];

        return tpa.Split(Path.PathSeparator)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key!, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }
}
