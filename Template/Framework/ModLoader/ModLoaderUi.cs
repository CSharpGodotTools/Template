using __TEMPLATE__.Mods;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

namespace __TEMPLATE__.Ui;

public class ModLoaderUi
{
    private readonly Dictionary<string, ModInfo> _mods = [];
    private readonly Dictionary<string, ManagedModRuntime> _managedMods = [];
    private readonly ILoggerService _logger;
    private readonly Services _services;
    private readonly GameServices _runtimeServices;

    public ModLoaderUi(ILoggerService logger, Services services, GameServices runtimeServices)
    {
        _logger = logger;
        _services = services;
        _runtimeServices = runtimeServices;
    }

    public Dictionary<string, ModInfo> GetMods()
    {
        return _mods;
    }

    public void LoadMods(Node node)
    {
        _mods.Clear();

        string modsPath = ProjectSettings.GlobalizePath("res://Mods");

        // Ensure "Mods" directory always exists
        Directory.CreateDirectory(modsPath);

        DirAccess dir = DirAccess.Open(modsPath);

        if (dir == null)
        {
            _logger.LogWarning("Failed to open Mods directory because it does not exist");
            return;
        }

        dir.ListDirBegin();

        string filename = dir.GetNext();

        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        while (filename != "")
        {
            if (!dir.CurrentIsDir())
            {
                filename = dir.GetNext();
                continue;
            }

            TryLoadModDirectory(node, modsPath, filename, options);
            filename = dir.GetNext();
        }

        dir.ListDirEnd();
        dir.Dispose();
    }

    private void TryLoadModDirectory(Node hostNode, string modsPath, string folderName, JsonSerializerOptions options)
    {
        string modRoot = $@"{modsPath}/{folderName}";
        string modJson = $@"{modRoot}/mod.json";

        if (!File.Exists(modJson))
        {
            _logger.LogWarning($"The mod folder '{folderName}' does not have a mod.json so it will not be loaded");
            return;
        }

        string jsonFileContents;
        try
        {
            jsonFileContents = File.ReadAllText(modJson);
        }
        catch (IOException exception)
        {
            _logger.LogWarning($"Failed to read '{modJson}': {exception.Message}");
            return;
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning($"Access denied while reading '{modJson}': {exception.Message}");
            return;
        }

        jsonFileContents = jsonFileContents.Replace("*", "Any");

        if (!TryDeserializeModInfo(modJson, jsonFileContents, options, out ModInfo? modInfo))
            return;

        modInfo!.Normalize();

        if (string.IsNullOrWhiteSpace(modInfo.Id))
        {
            _logger.LogWarning($"The mod folder '{folderName}' has an invalid or empty id and will be skipped");
            return;
        }

        if (_mods.ContainsKey(modInfo.Id))
        {
            _logger.LogWarning($"Duplicate mod id '{modInfo.Id}' was skipped");
            return;
        }

        _mods.Add(modInfo.Id, modInfo);

        string pckPath = $@"{modRoot}/mod.pck";
        if (File.Exists(pckPath))
        {
            bool success = ProjectSettings.LoadResourcePack(pckPath, replaceFiles: false);

            if (!success)
            {
                _logger.LogWarning($"Failed to load pck file for mod '{modInfo.Name}'");
            }
            else
            {
                TryInstantiateModScene(hostNode, modInfo);
            }
        }

        string dllPath = $@"{modRoot}/Mod.dll";
        if (File.Exists(dllPath))
        {
            TryLoadManagedMod(hostNode, modInfo, dllPath);
        }
    }

    private bool TryDeserializeModInfo(
        string modJsonPath,
        string jsonFileContents,
        JsonSerializerOptions options,
        out ModInfo? modInfo)
    {
        try
        {
            modInfo = JsonSerializer.Deserialize<ModInfo>(jsonFileContents, options);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning($"Failed to parse '{modJsonPath}': {exception.Message}");
            modInfo = new ModInfo();
            return false;
        }

        if (modInfo != null)
        {
            return true;
        }

        _logger.LogWarning($"The file '{modJsonPath}' is empty or malformed and was skipped");
        modInfo = new ModInfo();
        return false;
    }

    private void TryInstantiateModScene(Node hostNode, ModInfo modInfo)
    {
        string modScenePath = $"res://{modInfo.Author}/{modInfo.Id}/mod.tscn";
        PackedScene importedScene = ResourceLoader.Load<PackedScene>(modScenePath);

        if (importedScene != null)
        {
            Node modNode = SceneComposition.InstantiateAndConfigure<Node>(importedScene, _runtimeServices);
            hostNode.GetTree().Root.CallDeferred(Node.MethodName.AddChild, modNode);
        }
        else
        {
            _logger.LogWarning($"Failed to load mod.tscn for mod '{modInfo.Name}'. Expected path '{modScenePath}'.");
        }
    }

    private void TryLoadManagedMod(Node hostNode, ModInfo modInfo, string dllPath)
    {
        if (_managedMods.ContainsKey(modInfo.Id))
        {
            _logger.LogWarning($"Managed mod '{modInfo.Id}' is loaded already and was skipped");
            return;
        }

        try
        {
            ModLoadContext loadContext = new(dllPath);
            Assembly assembly = loadContext.LoadFromAssemblyPath(dllPath);
            IReadOnlyList<IModEntrypoint> entrypoints = ActivateEntrypoints(hostNode, modInfo, assembly);
            ManagedModRuntime runtime = new(loadContext, assembly, entrypoints);
            _managedMods.Add(modInfo.Id, runtime);
        }
        catch (FileNotFoundException exception)
        {
            _logger.LogErr(exception, $"Managed mod '{modInfo.Id}' assembly was not found");
        }
        catch (FileLoadException exception)
        {
            _logger.LogErr(exception, $"Managed mod '{modInfo.Id}' assembly could not be loaded");
        }
        catch (BadImageFormatException exception)
        {
            _logger.LogErr(exception, $"Managed mod '{modInfo.Id}' assembly is not a valid .NET assembly");
        }
        catch (Exception exception)
        {
            _logger.LogErr(exception, $"Failed to load managed mod '{modInfo.Id}'");
        }
    }

    private List<IModEntrypoint> ActivateEntrypoints(Node hostNode, ModInfo modInfo, Assembly assembly)
    {
        ModMetadata metadata = new(modInfo.Id, modInfo.Name, modInfo.Author, modInfo.ModVersion, modInfo.GameVersion);
        IModContext context = new ModContext(hostNode, metadata, _logger, _services);
        List<IModEntrypoint> entrypoints = [];
        Type entrypointType = typeof(IModEntrypoint);
        Type[] types = GetLoadableTypes(assembly, modInfo.Id);

        foreach (Type type in types.Where(type => !type.IsAbstract && !type.IsInterface && entrypointType.IsAssignableFrom(type)))
        {
            try
            {
                object? instance = Activator.CreateInstance(type);
                if (instance is IModEntrypoint entrypoint)
                {
                    entrypoint.OnLoad(context);
                    entrypoints.Add(entrypoint);
                }
            }
            catch (MissingMethodException exception)
            {
                _logger.LogErr(exception, $"Entrypoint '{type.FullName}' for mod '{modInfo.Id}' requires a public parameterless constructor");
            }
            catch (MemberAccessException exception)
            {
                _logger.LogErr(exception, $"Entrypoint '{type.FullName}' for mod '{modInfo.Id}' is not accessible");
            }
            catch (TargetInvocationException exception)
            {
                _logger.LogErr(exception, $"Entrypoint '{type.FullName}' for mod '{modInfo.Id}' threw during activation");
            }
            catch (Exception exception)
            {
                _logger.LogErr(exception, $"Failed to initialize entrypoint '{type.FullName}' for mod '{modInfo.Id}'");
            }
        }

        if (entrypoints.Count == 0)
        {
            _logger.LogWarning($"Managed mod '{modInfo.Id}' does not contain an IModEntrypoint implementation");
        }

        return entrypoints;
    }

    private Type[] GetLoadableTypes(Assembly assembly, string modId)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            foreach (Exception? loaderException in exception.LoaderExceptions)
            {
                if (loaderException is null)
                    continue;
                _logger.LogErr(loaderException, $"Managed mod '{modId}' failed to resolve one or more types");
            }

            Type[] loadableTypes = [.. (exception.Types ?? []).OfType<Type>()];
            return loadableTypes;
        }
    }

    private sealed class ManagedModRuntime(ModLoadContext loadContext, Assembly assembly, IReadOnlyList<IModEntrypoint> entrypoints)
    {
        public ModLoadContext LoadContext { get; } = loadContext;
        public Assembly Assembly { get; } = assembly;
        public IReadOnlyList<IModEntrypoint> Entrypoints { get; } = entrypoints;
    }

    private sealed class ModLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;
        private readonly Dictionary<string, Assembly> _sharedAssemblies;

        public ModLoadContext(string mainAssemblyPath)
            : base($"Mod::{Path.GetFileNameWithoutExtension(mainAssemblyPath)}::{Path.GetRandomFileName()}", isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
            _sharedAssemblies = new Dictionary<string, Assembly>(StringComparer.Ordinal)
            {
                [typeof(IModEntrypoint).Assembly.GetName().Name!] = typeof(IModEntrypoint).Assembly,
                [typeof(Node).Assembly.GetName().Name!] = typeof(Node).Assembly
            };
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (_sharedAssemblies.TryGetValue(assemblyName.Name!, out Assembly? sharedAssembly))
            {
                return sharedAssembly;
            }

            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (!string.IsNullOrWhiteSpace(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}

public class ModInfo
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string ModVersion { get; set; } = string.Empty;
    public string GameVersion { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    public Dictionary<string, string> Dependencies { get; set; } = [];
    public Dictionary<string, string> Incompatibilities { get; set; } = [];

    public void Normalize()
    {
        Name = string.IsNullOrWhiteSpace(Name) ? Id : Name;
        Author = string.IsNullOrWhiteSpace(Author) ? "Unknown" : Author;
        ModVersion = string.IsNullOrWhiteSpace(ModVersion) ? "Unknown" : ModVersion;
        GameVersion = string.IsNullOrWhiteSpace(GameVersion) ? "Unknown" : GameVersion;
        Description ??= string.Empty;
        Dependencies ??= [];
        Incompatibilities ??= [];
    }
}
