using Godot;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PacketGen.Tests;

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

    public GeneratorTestBuilder(string testSource, string? generatedFile = null)
    {
        _sources.Add(testSource);
        _generatedFile = generatedFile;
    }

    public GeneratorTestBuilder<TGenerator> AddMetadataReference(Type type)
    {
        _references.Add(type.Assembly.Location);
        return this;
    }

    public GeneratorTestBuilder<TGenerator> AddSource(string source)
    {
        _sources.Add(source);
        return this;
    }

    public GeneratorTestBuilder<TGenerator> AddTrustedPlatformReferences(params string[] assemblyNames)
    {
        foreach (string name in assemblyNames)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _trustedPlatformAssemblies.Add(name);
        }

        return this;
    }

    public GeneratorTestBuilder<TGenerator> WithGeneratedFile(string fileName)
    {
        _generatedFile = fileName;
        return this;
    }

    public GeneratorTestOptions Build()
    {
        if (string.IsNullOrWhiteSpace(_generatedFile))
            throw new InvalidOperationException("Generated file name must be set.");

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
