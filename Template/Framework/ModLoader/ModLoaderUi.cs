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

/// <summary>
/// Discovers, validates, and initializes filesystem-based mods for UI and runtime activation.
/// </summary>
public class ModLoaderUi
{
    private readonly Dictionary<string, ModInfo> _mods = [];
    private readonly Dictionary<string, ManagedModRuntime> _managedMods = [];
    private readonly ILoggerService _logger;
    private readonly Services _services;
    private readonly GameServices _runtimeServices;

    /// <summary>
    /// Creates a mod loader helper bound to logger, service locator, and runtime composition services.
    /// </summary>
    /// <param name="logger">Logger used for diagnostics and load failures.</param>
    /// <param name="services">Scoped service locator exposed to managed mods.</param>
    /// <param name="runtimeServices">Runtime services used for scene composition.</param>
    public ModLoaderUi(ILoggerService logger, Services services, GameServices runtimeServices)
    {
        _logger = logger;
        _services = services;
        _runtimeServices = runtimeServices;
    }

    /// <summary>
    /// Gets loaded mod metadata keyed by normalized mod identifier.
    /// </summary>
    /// <returns>Mutable metadata map of discovered mods.</returns>
    public Dictionary<string, ModInfo> GetMods()
    {
        return _mods;
    }

    /// <summary>
    /// Scans the Mods directory and attempts to load each valid mod package and managed assembly.
    /// </summary>
    /// <param name="node">Host node used for attaching composed mod scenes.</param>
    public void LoadMods(Node node)
    {
        _mods.Clear();

        string modsPath = ProjectSettings.GlobalizePath("res://Mods");

        // Ensure "Mods" directory always exists
        Directory.CreateDirectory(modsPath);

        DirAccess dir = DirAccess.Open(modsPath);

        // Abort load pass when the Mods directory cannot be opened.
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
            // Skip non-directory entries in the Mods root.
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

    /// <summary>
    /// Attempts to load a single mod directory including manifest, optional scene pack, and managed assembly.
    /// </summary>
    /// <param name="hostNode">Host node used for scene attachment.</param>
    /// <param name="modsPath">Absolute Mods folder path.</param>
    /// <param name="folderName">Current mod folder name.</param>
    /// <param name="options">JSON options used for manifest deserialization.</param>
    private void TryLoadModDirectory(Node hostNode, string modsPath, string folderName, JsonSerializerOptions options)
    {
        string modRoot = $"{modsPath}/{folderName}";
        string modJson = $"{modRoot}/mod.json";

        // Require a manifest file before attempting to load this folder as a mod.
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

        // Keep wildcard-like author values from breaking strict typed metadata fields.
        jsonFileContents = jsonFileContents.Replace("*", "Any");

        // Skip this folder when manifest parsing fails.
        if (!TryDeserializeModInfo(modJson, jsonFileContents, options, out ModInfo? modInfo))
            return;

        modInfo!.Normalize();

        // Require a normalized non-empty mod identifier.
        if (string.IsNullOrWhiteSpace(modInfo.Id))
        {
            _logger.LogWarning($"The mod folder '{folderName}' has an invalid or empty id and will be skipped");
            return;
        }

        // Enforce unique mod identifiers across all loaded mods.
        if (_mods.ContainsKey(modInfo.Id))
        {
            _logger.LogWarning($"Duplicate mod id '{modInfo.Id}' was skipped");
            return;
        }

        _mods.Add(modInfo.Id, modInfo);

        // Resource pack loading happens first so referenced assets become available to mod scenes.
        string pckPath = $"{modRoot}/mod.pck";

        // Load optional resource pack when present.
        if (File.Exists(pckPath))
        {
            bool success = ProjectSettings.LoadResourcePack(pckPath, replaceFiles: false);

            // Report failed resource-pack load attempts.
            if (!success)
            {
                _logger.LogWarning($"Failed to load pck file for mod '{modInfo.Name}'");
            }
            else
            {
                TryInstantiateModScene(hostNode, modInfo);
            }
        }

        // Managed entrypoints are optional and run after metadata normalization and pack loading.
        string dllPath = $"{modRoot}/Mod.dll";

        // Load optional managed assembly when present.
        if (File.Exists(dllPath))
            TryLoadManagedMod(hostNode, modInfo, dllPath);
    }

    /// <summary>
    /// Deserializes a mod manifest and reports parse failures through the logger.
    /// </summary>
    /// <param name="modJsonPath">Path to the manifest file.</param>
    /// <param name="jsonFileContents">Raw manifest JSON text.</param>
    /// <param name="options">JSON serializer options.</param>
    /// <param name="modInfo">Deserialized mod info output when successful.</param>
    /// <returns><see langword="true"/> when deserialization produces a non-null manifest model.</returns>
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

        // Treat a non-null model as a successful deserialization.
        if (modInfo != null)
            return true;

        _logger.LogWarning($"The file '{modJsonPath}' is empty or malformed and was skipped");
        modInfo = new ModInfo();
        return false;
    }

    /// <summary>
    /// Instantiates and schedules a mod scene when a valid mod scene resource is found.
    /// </summary>
    /// <param name="hostNode">Host node used for deferred root attachment.</param>
    /// <param name="modInfo">Mod metadata describing author/id resource paths.</param>
    private void TryInstantiateModScene(Node hostNode, ModInfo modInfo)
    {
        string modScenePath = $"res://{modInfo.Author}/{modInfo.Id}/mod.tscn";
        PackedScene importedScene = ResourceLoader.Load<PackedScene>(modScenePath);

        // Instantiate and attach mod scene only when resource loading succeeds.
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

    /// <summary>
    /// Loads and activates a managed mod assembly plus discovered entrypoints.
    /// </summary>
    /// <param name="hostNode">Host node exposed to managed mod context.</param>
    /// <param name="modInfo">Mod metadata for diagnostics and runtime keys.</param>
    /// <param name="dllPath">Path to the managed mod assembly.</param>
    private void TryLoadManagedMod(Node hostNode, ModInfo modInfo, string dllPath)
    {
        // Avoid loading the same managed mod runtime more than once.
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

    /// <summary>
    /// Creates and runs all <see cref="IModEntrypoint"/> implementations found in an assembly.
    /// </summary>
    /// <param name="hostNode">Host node exposed to mod entrypoints.</param>
    /// <param name="modInfo">Mod metadata used to build runtime context.</param>
    /// <param name="assembly">Managed assembly to inspect.</param>
    /// <returns>Activated entrypoint instances.</returns>
    private List<IModEntrypoint> ActivateEntrypoints(Node hostNode, ModInfo modInfo, Assembly assembly)
    {
        ModMetadata metadata = new(modInfo.Id, modInfo.Name, modInfo.Author, modInfo.ModVersion, modInfo.GameVersion);
        IModContext context = new ModContext(hostNode, metadata, _logger, _services);
        List<IModEntrypoint> entrypoints = [];
        Type entrypointType = typeof(IModEntrypoint);
        Type[] types = GetLoadableTypes(assembly, modInfo.Id);

        // Only concrete implementations are eligible for activation.
        foreach (Type type in types.Where(type => !type.IsAbstract && !type.IsInterface && entrypointType.IsAssignableFrom(type)))
        {
            try
            {
                object? instance = Activator.CreateInstance(type);

                // Activate only types that implement IModEntrypoint.
                if (instance is IModEntrypoint entrypoint)
                {
                    // Run user initialization immediately so mods can register their hooks.
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

        // Warn when no entrypoint implementations were found.
        if (entrypoints.Count == 0)
            _logger.LogWarning($"Managed mod '{modInfo.Id}' does not contain an IModEntrypoint implementation");

        return entrypoints;
    }

    /// <summary>
    /// Returns loadable assembly types while gracefully handling partial type-load failures.
    /// </summary>
    /// <param name="assembly">Assembly being inspected.</param>
    /// <param name="modId">Mod identifier used in diagnostic output.</param>
    /// <returns>All types that successfully loaded.</returns>
    private Type[] GetLoadableTypes(Assembly assembly, string modId)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            // Surface each loader exception to aid dependency troubleshooting in user mods.
            foreach (Exception? loaderException in exception.LoaderExceptions)
            {
                // Skip null loader-exception slots emitted by the runtime.
                if (loaderException is null)
                    continue;
                _logger.LogErr(loaderException, $"Managed mod '{modId}' failed to resolve one or more types");
            }

            Type[] loadableTypes = [.. (exception.Types ?? []).OfType<Type>()];
            return loadableTypes;
        }
    }

    /// <summary>
    /// Tracks the runtime state associated with a loaded managed mod assembly.
    /// </summary>
    /// <param name="loadContext">Collectible load context used for mod assemblies.</param>
    /// <param name="assembly">Loaded managed assembly for the mod.</param>
    /// <param name="entrypoints">Discovered entrypoint instances for the mod.</param>
    private sealed class ManagedModRuntime(ModLoadContext loadContext, Assembly assembly, IReadOnlyList<IModEntrypoint> entrypoints)
    {
        public ModLoadContext LoadContext { get; } = loadContext;
        public Assembly Assembly { get; } = assembly;
        public IReadOnlyList<IModEntrypoint> Entrypoints { get; } = entrypoints;
    }

    /// <summary>
    /// Isolated assembly load context used to load managed mod dependencies.
    /// </summary>
    private sealed class ModLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;
        private readonly Dictionary<string, Assembly> _sharedAssemblies;

        /// <summary>
        /// Creates a collectible load context rooted at a managed mod assembly path.
        /// </summary>
        /// <param name="mainAssemblyPath">Path to the mod's main assembly.</param>
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

        /// <summary>
        /// Resolves assemblies from shared host references first, then from the mod dependency graph.
        /// </summary>
        /// <param name="assemblyName">Assembly name requested by the runtime loader.</param>
        /// <returns>Resolved assembly or null when unresolved.</returns>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Share host assemblies so type identity stays consistent across mod boundaries.
            if (_sharedAssemblies.TryGetValue(assemblyName.Name!, out Assembly? sharedAssembly))
                return sharedAssembly;

            // Fall back to mod-local dependency resolution for third-party assemblies.
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            // Load dependency from resolved on-disk path when available.
            if (!string.IsNullOrWhiteSpace(assemblyPath))
                return LoadFromAssemblyPath(assemblyPath);

            return null;
        }
    }
}

/// <summary>
/// Serializable mod manifest model loaded from each mod's <c>mod.json</c> file.
/// </summary>
public class ModInfo
{
    /// <summary>
    /// Display name shown in the mod loader UI.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier used as the primary mod key.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Declared mod version string.
    /// </summary>
    public string ModVersion { get; set; } = string.Empty;

    /// <summary>
    /// Target game version string for compatibility checks.
    /// </summary>
    public string GameVersion { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description displayed in the loader UI.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Author or team name for the mod.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Required mod IDs and their expected versions.
    /// </summary>
    public Dictionary<string, string> Dependencies { get; set; } = [];

    /// <summary>
    /// Mod IDs and versions known to be incompatible.
    /// </summary>
    public Dictionary<string, string> Incompatibilities { get; set; } = [];

    /// <summary>
    /// Normalizes missing manifest fields to safe defaults.
    /// </summary>
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
