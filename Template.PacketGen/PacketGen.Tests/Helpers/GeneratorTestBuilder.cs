using Microsoft.CodeAnalysis;

namespace PacketGen.Tests;

/// <summary>
/// Fluent builder for constructing <see cref="GeneratorTestOptions"/> instances.
/// </summary>
/// <typeparam name="TGenerator">Generator type under test.</typeparam>
internal sealed class GeneratorTestBuilder<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    private readonly List<string> _sources = [];
    private readonly HashSet<string> _references = new(StringComparer.OrdinalIgnoreCase)
    {
        typeof(object).Assembly.Location,
        typeof(Enumerable).Assembly.Location,
        typeof(Godot.Vector2).Assembly.Location,
        typeof(Godot.Vector3).Assembly.Location
    };

    private readonly List<string> _trustedPlatformAssemblies = [
        "System.Runtime.dll",
        "System.Collections.dll"
    ];

    private string? _generatedFile;

    /// <summary>
    /// Creates a test-options builder with initial source and optional generated file name.
    /// </summary>
    /// <param name="testSource">Primary test source.</param>
    /// <param name="generatedFile">Optional expected generated file name.</param>
    public GeneratorTestBuilder(string testSource, string? generatedFile = null)
    {
        _sources.Add(testSource);
        _generatedFile = generatedFile;
    }

    /// <summary>
    /// Adds metadata reference for the assembly containing <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type whose assembly should be referenced.</param>
    /// <returns>Current builder.</returns>
    public GeneratorTestBuilder<TGenerator> AddMetadataReference(Type type)
    {
        _references.Add(type.Assembly.Location);
        return this;
    }

    /// <summary>
    /// Adds an additional source text to the test compilation.
    /// </summary>
    /// <param name="source">Source text to add.</param>
    /// <returns>Current builder.</returns>
    public GeneratorTestBuilder<TGenerator> AddSource(string source)
    {
        _sources.Add(source);
        return this;
    }

    /// <summary>
    /// Adds trusted platform assembly names used for compilation references.
    /// </summary>
    /// <param name="assemblyNames">Assembly file names (for example <c>System.Runtime.dll</c>).</param>
    /// <returns>Current builder.</returns>
    public GeneratorTestBuilder<TGenerator> AddTrustedPlatformReferences(params string[] assemblyNames)
    {
        foreach (string name in assemblyNames)
        {
            // Ignore blank assembly names to keep reference list valid.
            if (!string.IsNullOrWhiteSpace(name))
                _trustedPlatformAssemblies.Add(name);
        }

        return this;
    }

    /// <summary>
    /// Sets expected generated file name.
    /// </summary>
    /// <param name="fileName">Expected generated file name.</param>
    /// <returns>Current builder.</returns>
    public GeneratorTestBuilder<TGenerator> WithGeneratedFile(string fileName)
    {
        _generatedFile = fileName;
        return this;
    }

    /// <summary>
    /// Builds an immutable options payload for a generator test run.
    /// </summary>
    /// <returns>Built test options.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when generated file name or required sources are missing.
    /// </exception>
    public GeneratorTestOptions Build()
    {
        // Generated-file expectations are required for test assertions.
        if (string.IsNullOrWhiteSpace(_generatedFile))
            throw new InvalidOperationException("Generated file name must be set.");

        // At least one source file is required to compile a generator test.
        if (_sources.Count == 0)
            throw new InvalidOperationException("At least one source must be provided.");

        return new GeneratorTestOptions(
            _generatedFile!,
            [.. _sources],
            [.. _references],
            [.. _trustedPlatformAssemblies]
        );
    }
}
