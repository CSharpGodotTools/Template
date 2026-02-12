using Godot;
using GodotUtils;
using GodotUtils.RegEx;
using System;
using System.IO;

namespace Framework.Setup;

public static class SetupUtils
{
    private const string RawTemplateNamespace = "__TEMPLATE__";
    private const string TemplateName = "Template";
    private const string TemplateMainSceneUid = "uid://dnmu3cujgayk2";

    /// <summary>
    /// GDUnit4 addon creates 2 folders that I do not want showing up in the git root.
    /// Instead of committing the .gdignore files to git creating folder spam, we create them
    /// after the setup. The .gdignore files ensure these files are not visible in the editor.
    /// </summary>
    /// <param name="projectRoot"></param>
    public static void EnsureGDIgnoreFilesInGDUnitTestFolders(string projectRoot)
    {
        string[] folders =
        [
            "TestResults",
            "gdunit4_testadapter_v5"
        ];

        foreach (string folder in folders)
        {
            string fullPath = Path.Combine(projectRoot, folder);

            // Create the folder if it does not exist
            Directory.CreateDirectory(fullPath);

            // Create .gdignore so Godot hides the folder in the editor
            string gdIgnorePath = Path.Combine(fullPath, ".gdignore");

            if (!File.Exists(gdIgnorePath))
            {
                File.WriteAllText(gdIgnorePath, string.Empty);
            }
        }
    }

    /// <summary>
    /// Verifies the game name is not using the raw template namespace and not any existing class name in the project.
    /// </summary>
    public static bool IsGameNameBad(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            GD.PrintErr("The game name cannot be empty.");
            return true;
        }

        // Prevent game name being the same as the reserved raw template namespace name
        if (name.Equals(RawTemplateNamespace, System.StringComparison.OrdinalIgnoreCase))
        {
            GD.PrintErr($"{RawTemplateNamespace} is a reserved name.");
            return true;
        }

        // Prevent game name being the same as an existing class name in the project
        if (EqualsExistingClassName(name))
        {
            GD.PrintErr($"Namespace {name} is the same name as {name}.cs");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns whether or not <paramref name="name"/> is equal to an existing script name in the project.
    /// </summary>
    public static bool EqualsExistingClassName(string name)
    {
        bool namespaceSameAsClassName = false;

        DirectoryUtils.Traverse("res://", entry =>
        {
            if (entry.FileName.Equals(name + ".cs", StringComparison.OrdinalIgnoreCase))
            {
                namespaceSameAsClassName = true;
                return TraverseDecision.Stop;
            }

            return TraverseDecision.Continue;
        });

        return namespaceSameAsClassName;
    }

    /// <summary>
    /// Changes the 'run/main_scene' to <paramref name="sceneName"/> in the project.godot file at <paramref name="projectRoot"/>.
    /// </summary>
    public static void SetMainScene(string projectRoot, string sceneName)
    {
        string text = File.ReadAllText(Path.Combine(projectRoot, "project.godot"));

        text = text.Replace(
           $"run/main_scene=\"{TemplateMainSceneUid}\"",
           $"run/main_scene=\"{GetUIdFromSceneFile(Path.Combine(projectRoot, $"{sceneName}.tscn"))}\"");

        File.WriteAllText(Path.Combine(projectRoot, "project.godot"), text);
    }

    /// <summary>
    /// Replaces all instances of the keyword "Template" with the new
    /// specified game name in several project files.
    /// </summary>
    public static void RenameProjectFiles(string projectRoot, string name)
    {
        RenameCSProjFile(projectRoot, name);
        RenameSolutionFile(projectRoot, name);
        RenameProjectGodotFile(projectRoot, name);
    }

    /// <summary>
    /// Renames 'project/assembly_name' and 'config/name' to <paramref name="name"/> in the project.godot at <paramref name="projectRoot"/>.
    /// </summary>
    private static void RenameProjectGodotFile(string projectRoot, string name)
    {
        string fullPath = Path.Combine(projectRoot, "project.godot");
        string text = File.ReadAllText(fullPath);

        // Change assembly name
        text = text.Replace(
            $"project/assembly_name=\"{TemplateName}\"",
            $"project/assembly_name=\"{name}\"");

        // Change config name
        text = text.Replace(
            $"config/name=\"{TemplateName}\"",
            $"config/name=\"{name}\""
            );

        // Remove SetupPlugin addon
        text = text.Replace(
            "\"res://addons/SetupPlugin/plugin.cfg\", ",
            "");

        File.WriteAllText(fullPath, text);
    }

    /// <summary>
    /// Renames the .sln file at <paramref name="projectRoot"/> to <paramref name="name"/> and replaces all occurences of "Template" in the .sln with <paramref name="name"/>.
    /// </summary>
    private static void RenameSolutionFile(string projectRoot, string name)
    {
        string fullPath = Path.Combine(projectRoot, $"{TemplateName}.sln");
        string text = File.ReadAllText(fullPath);
        text = text.Replace(TemplateName, name);
        File.Delete(fullPath);
        File.WriteAllText(Path.Combine(projectRoot, name + ".sln"), text);
    }

    /// <summary>
    /// Renames the .csproj file at <paramref name="projectRoot"/> to <paramref name="name"/> and replaces the RootNamespace option with <paramref name="name"/>.
    /// </summary>
    private static void RenameCSProjFile(string projectRoot, string name)
    {
        string fullPath = Path.Combine(projectRoot, $"{TemplateName}.csproj");
        string text = File.ReadAllText(fullPath);
        text = text.Replace($"<RootNamespace>{TemplateName}</RootNamespace>", $"<RootNamespace>{name}</RootNamespace>");
        File.Delete(fullPath);
        File.WriteAllText(Path.Combine(projectRoot, name + ".csproj"), text);
    }

    /// <summary>
    /// Renames the default "__TEMPLATE__" namespace to the new specified game name in all scripts.
    /// Note that this assumes no one will use the namespace name "__TEMPLATE__".
    /// </summary>
    public static void RenameAllNamespaces(string projectRoot, string newNamespaceName)
    {
        DirectoryUtils.Traverse(projectRoot, entry =>
        {
            // Ignore these directories
            switch (entry.FileName.ToLower())
            {
                case ".godot":
                case "addons":
                case "godotutils":
                case "mods":
                case "framework":
                    return TraverseDecision.SkipChildren;
            }

            // Modify all scripts
            if (entry.FileName.EndsWith(".cs"))
            {
                string text = File.ReadAllText(entry.FullPath);

                text = text.Replace($"namespace {RawTemplateNamespace}", $"namespace {newNamespaceName}");
                text = text.Replace($"using {RawTemplateNamespace}", $"using {newNamespaceName}");
                text = text.Replace($"{RawTemplateNamespace}.", $"{newNamespaceName}.");

                File.WriteAllText(entry.FullPath, text);
            }

            return TraverseDecision.Continue;
        });
    }

    /// <summary>
    /// Retrieves the Uid string from a .tscn scene file at <paramref name="projectRoot"/>.
    /// </summary>
    public static string GetUIdFromSceneFile(string projectRoot)
    {
        string uid;

        using StreamReader reader = new(projectRoot);

        // Assuming the scene uid is on the first line in the file
        string line = reader.ReadLine();

        // [gd_scene load_steps=35 format=4 uid="uid://btkfgi3rc5wm1"]
        if (line.Contains("gd_scene"))
        {
            uid = line.Split("uid=")[1].Split('"')[1];
            return uid;
        }

        return null;
    }

    /// <summary>
    /// Formats <paramref name="name"/> by trimming, ensuring first char is uppercase and removing all spaces
    /// </summary>
    public static string FormatGameName(string name)
    {
        return name.Trim().FirstCharToUpper().Replace(" ", "");
    }

    /// <summary>
    /// Checks if <paramref name="str"/> is alpha numeric with spaces being allowed.
    /// </summary>
    public static bool IsAlphaNumericAndAllowSpaces(string str)
    {
        return RegexUtils.AlphaNumericAndSpaces().IsMatch(str);
    }
}
