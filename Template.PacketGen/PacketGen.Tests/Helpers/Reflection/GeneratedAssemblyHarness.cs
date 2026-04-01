using Microsoft.CodeAnalysis;
using System.Reflection;

namespace PacketGen.Tests;

/// <summary>
/// Builds and inspects generated test assemblies for PacketGen.
/// </summary>
internal sealed class GeneratedAssemblyHarness
{
    /// <summary>
    /// Initializes a harness instance with generator results, compiled assembly, and generated file storage.
    /// </summary>
    /// <param name="result">Generator test run result.</param>
    /// <param name="assembly">Compiled generated assembly.</param>
    /// <param name="fileStore">Store containing generated artifacts.</param>
    private GeneratedAssemblyHarness(GeneratorTestRunResult result, Assembly assembly, IGeneratedFileStore fileStore)
    {
        Result = result;
        Assembly = assembly;
        FileStore = fileStore;
    }

    /// <summary>
    /// Gets the generator run result.
    /// </summary>
    public GeneratorTestRunResult Result { get; }

    /// <summary>
    /// Gets the compiled generated assembly.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// Gets the file store used for generated artifacts.
    /// </summary>
    public IGeneratedFileStore FileStore { get; }

    /// <summary>
    /// Builds and compiles generated output for a test source.
    /// </summary>
    /// <typeparam name="TGenerator">Generator type under test.</typeparam>
    /// <param name="testSource">Input source code.</param>
    /// <param name="generatedFile">Expected generated file name.</param>
    /// <returns>Generated assembly harness.</returns>
    public static GeneratedAssemblyHarness Build<TGenerator>(string testSource, string generatedFile)
        where TGenerator : IIncrementalGenerator, new()
    {
        GeneratorTestOptions options = new GeneratorTestBuilder<TGenerator>(testSource)
            .WithGeneratedFile(generatedFile)
            .Build();

        GeneratorTestRunResult? result = GeneratorTestRunner<TGenerator>.Run(options);

        Assert.That(result, Is.Not.Null, $"{generatedFile} failed to generate.");

        GeneratedFileStore fileStore = new();
        fileStore.Write(result.GeneratedFile, result.GeneratedSource);

        Assembly assembly = GeneratedAssemblyCompiler.Compile(result, fileStore);

        return new GeneratedAssemblyHarness(result, assembly, fileStore);
    }

    /// <summary>
    /// Returns the requested type or fails the test when missing.
    /// </summary>
    /// <param name="fullName">Fully qualified type name.</param>
    /// <returns>Resolved type.</returns>
    public Type GetTypeOrFail(string fullName)
    {
        Type? type = Assembly.GetType(fullName);
        Assert.That(type, Is.Not.Null, $"Type '{fullName}' was not found in generated assembly.");
        return type!;
    }

    /// <summary>
    /// Creates an instance of the provided type or fails the test.
    /// </summary>
    /// <param name="type">Type to instantiate.</param>
    /// <returns>Created instance.</returns>
    public static object CreateInstance(Type type)
    {
        object? instance = Activator.CreateInstance(type);
        Assert.That(instance, Is.Not.Null, $"Failed to create instance of '{type.FullName}'.");
        return instance!;
    }

    /// <summary>
    /// Creates a packet writer instance from the generated assembly.
    /// </summary>
    /// <returns>Packet writer instance.</returns>
    public object CreateWriter()
    {
        Type writerType = GetTypeOrFail(PacketGenTestConstants.PacketWriterFullName);
        return CreateInstance(writerType);
    }

    /// <summary>
    /// Reads the Values property from a packet writer instance.
    /// </summary>
    /// <param name="writer">Writer instance to inspect.</param>
    /// <returns>Captured writer values.</returns>
    public static IReadOnlyList<object?> GetWriterValues(object writer)
    {
        var prop = writer.GetType().GetProperty("Values", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(prop, Is.Not.Null, "PacketWriter.Values property was not found.");

        object? value = prop!.GetValue(writer);
        Assert.That(value, Is.Not.Null, "PacketWriter.Values is null.");

        return (IReadOnlyList<object?>)value!;
    }

    /// <summary>
    /// Creates a packet reader instance using the provided values.
    /// </summary>
    /// <param name="values">Values to seed the reader.</param>
    /// <returns>Packet reader instance.</returns>
    public object CreateReader(IEnumerable<object?> values)
    {
        Type readerType = GetTypeOrFail(PacketGenTestConstants.PacketReaderFullName);
        object? reader = Activator.CreateInstance(readerType, [values]);
        Assert.That(reader, Is.Not.Null, "Failed to create PacketReader with provided values.");
        return reader!;
    }
}

