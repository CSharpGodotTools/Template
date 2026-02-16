using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PacketGen.Tests;

internal sealed class GeneratedAssemblyHarness
{
    private GeneratedAssemblyHarness(GeneratorTestRunResult result, Assembly assembly, IGeneratedFileStore fileStore)
    {
        Result = result;
        Assembly = assembly;
        FileStore = fileStore;
    }

    public GeneratorTestRunResult Result { get; }
    public Assembly Assembly { get; }
    public IGeneratedFileStore FileStore { get; }

    public static GeneratedAssemblyHarness Build<TGenerator>(string testSource, string generatedFile)
        where TGenerator : IIncrementalGenerator, new()
    {
        GeneratorTestOptions options = new GeneratorTestBuilder<TGenerator>(testSource)
            .WithGeneratedFile(generatedFile)
            .Build();

        GeneratorTestRunResult? result = GeneratorTestRunner<TGenerator>.Run(options);

        Assert.That(result, Is.Not.Null, $"{generatedFile} failed to generate.");

        IGeneratedFileStore fileStore = new GeneratedFileStore();
        fileStore.Write(result.GeneratedFile, result.GeneratedSource);

        Assembly assembly = GeneratedAssemblyCompiler.Compile(result, fileStore);

        return new GeneratedAssemblyHarness(result, assembly, fileStore);
    }

    public Type GetTypeOrFail(string fullName)
    {
        Type? type = Assembly.GetType(fullName);
        Assert.That(type, Is.Not.Null, $"Type '{fullName}' was not found in generated assembly.");
        return type!;
    }

    public object CreateInstance(Type type)
    {
        object? instance = Activator.CreateInstance(type);
        Assert.That(instance, Is.Not.Null, $"Failed to create instance of '{type.FullName}'.");
        return instance!;
    }

    public object CreateWriter()
    {
        Type writerType = GetTypeOrFail("Framework.Netcode.PacketWriter");
        return CreateInstance(writerType);
    }

    public IReadOnlyList<object?> GetWriterValues(object writer)
    {
        var prop = writer.GetType().GetProperty("Values", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(prop, Is.Not.Null, "PacketWriter.Values property was not found.");

        object? value = prop!.GetValue(writer);
        Assert.That(value, Is.Not.Null, "PacketWriter.Values is null.");

        return (IReadOnlyList<object?>)value!;
    }

    public object CreateReader(IEnumerable<object?> values)
    {
        Type readerType = GetTypeOrFail("Framework.Netcode.PacketReader");
        object? reader = Activator.CreateInstance(readerType, new object?[] { values });
        Assert.That(reader, Is.Not.Null, "Failed to create PacketReader with provided values.");
        return reader!;
    }
}


