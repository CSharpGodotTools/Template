using Godot;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

namespace GodotUtils.UI;

public class ModLoaderUI
{
    private Dictionary<string, ModInfo> _mods = [];

    public Dictionary<string, ModInfo> GetMods()
    {
        return _mods;
    }

    public void LoadMods(Node node)
    {
        string modsPath = ProjectSettings.GlobalizePath("res://Mods");

        // Ensure "Mods" directory always exists
        Directory.CreateDirectory(modsPath);

        DirAccess dir = DirAccess.Open(modsPath);

        if (dir == null)
        {
            Logger.LogWarning("Failed to open Mods directory has it does not exist");
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
                goto Next;
            }

            string modRoot = $@"{modsPath}/{filename}";
            string modJson = $@"{modRoot}/mod.json";

            if (!File.Exists(modJson))
            {
                Logger.LogWarning($"The mod folder '{filename}' does not have a mod.json so it will not be loaded");
                goto Next;
            }

            string jsonFileContents = File.ReadAllText(modJson);

            jsonFileContents = jsonFileContents.Replace("*", "Any");

            ModInfo modInfo = JsonSerializer.Deserialize<ModInfo>(jsonFileContents, options);

            if (_mods.ContainsKey(modInfo.Id))
            {
                Logger.LogWarning($"Duplicate mod id '{modInfo.Id}' was skipped");
                goto Next;
            }

            _mods.Add(modInfo.Id, modInfo);

            // Load dll
            string dllPath = $@"{modRoot}/Mod.dll";

            if (File.Exists(dllPath))
            {
                AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(typeof(Godot.Bridge.ScriptManagerBridge).Assembly);
                Assembly assembly = context.LoadFromAssemblyPath(dllPath);
                Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);
            }

            // Load pck
            string pckPath = $@"{modRoot}/mod.pck";

            if (File.Exists(pckPath))
            {
                bool success = ProjectSettings.LoadResourcePack(pckPath, replaceFiles: true);

                if (!success)
                {
                    Logger.LogWarning($"Failed to load pck file for mod '{modInfo.Name}'");
                    goto Next;
                }

                string modScenePath = $"res://{modInfo.Author}/{modInfo.Id}/mod.tscn";

                PackedScene importedScene = (PackedScene)ResourceLoader.Load(modScenePath);

                if (importedScene == null)
                {
                    Logger.LogWarning($"Failed to load mod.tscn for mod '{modInfo.Name}'");
                    goto Next;
                }

                Node mod = importedScene.Instantiate<Node>();
                node.GetTree().Root.CallDeferred(Node.MethodName.AddChild, mod);
            }

        Next:
            filename = dir.GetNext();
        }

        dir.ListDirEnd();
        dir.Dispose();
    }
}

public class ModInfo
{
    public string Name        { get; set; }
    public string Id          { get; set; }
    public string ModVersion  { get; set; }
    public string GameVersion { get; set; }
    public string Description { get; set; }
    public string Author      { get; set; }

    public Dictionary<string, string> Dependencies      { get; set; }
    public Dictionary<string, string> Incompatibilities { get; set; }
}
