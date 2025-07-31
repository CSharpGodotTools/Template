using Godot;
using GodotUtils;
using System.IO;
using System.Text.RegularExpressions;

namespace __TEMPLATE__.Setup;

public static partial class SetupUtils
{
    public static void SetMainScene(string path, string sceneName)
    {
        string text = File.ReadAllText(Path.Combine(path, "project.godot"));

        text = text.Replace(
            "run/main_scene=\"uid://dnmu3cujgayk2\"",
           $"run/main_scene=\"{SetupUtils.GetUIdFromSceneFile(Path.Combine(path, $"{sceneName}.tscn"))}\"");

        File.WriteAllText(Path.Combine(path, "project.godot"), text);
    }

    /// <summary>
    /// Replaces all instances of the keyword "Template" with the new
    /// specified game name in several project files.
    /// </summary>
    public static void RenameProjectFiles(string path, string name)
    {
        RenameCSProjFile(path, name);
        RenameSolutionFile(path, name);
        RenameProjectGodotFile(path, name);
    }

    private static void RenameProjectGodotFile(string path, string name)
    {
        string fullPath = Path.Combine(path, "project.godot");
        string text = File.ReadAllText(fullPath);

        text = text.Replace(
            "project/assembly_name=\"Template\"",
            $"project/assembly_name=\"{name}\"");

        text = text.Replace(
            "config/name=\"Template\"",
            $"config/name=\"{name}\""
            );

        File.WriteAllText(fullPath, text);
    }

    private static void RenameSolutionFile(string path, string name)
    {
        string fullPath = Path.Combine(path, "Template.sln");
        string text = File.ReadAllText(fullPath);
        text = text.Replace("Template", name);
        File.Delete(fullPath);
        File.WriteAllText(Path.Combine(path, name + ".sln"), text);
    }

    private static void RenameCSProjFile(string path, string name)
    {
        string fullPath = Path.Combine(path, "Template.csproj");
        string text = File.ReadAllText(fullPath);
        text = text.Replace("<RootNamespace>Template</RootNamespace>", $"<RootNamespace>{name}</RootNamespace>");
        File.Delete(fullPath);
        File.WriteAllText(Path.Combine(path, name + ".csproj"), text);
    }

    /// <summary>
    /// Renames the default "__TEMPLATE__" namespace to the new specified game name in all scripts.
    /// Note that this assumes no one will use the namespace name "__TEMPLATE__".
    /// </summary>
    public static void RenameAllNamespaces(string path, string newNamespaceName)
    {
        DirectoryUtils.Traverse(path, RenameNamespaces);

        void RenameNamespaces(string fullFilePath)
        {
            // Ignore these directories
            switch (Path.GetDirectoryName(fullFilePath))
            {
                case ".godot":
                case "GodotUtils":
                case "addons":
                    return;
            }

            // Modify all scripts
            if (fullFilePath.EndsWith(".cs"))
            {
                // Do not modify this script
                if (!fullFilePath.EndsWith("Setup.cs"))
                {
                    const string oldNamespaceName = "__TEMPLATE__";

                    string text = File.ReadAllText(fullFilePath);

                    text = text.Replace($"namespace {oldNamespaceName}", $"namespace {newNamespaceName}");
                    text = text.Replace($"using {oldNamespaceName}", $"using {newNamespaceName}");
                    text = text.Replace($"{oldNamespaceName}.", $"{newNamespaceName}.");

                    File.WriteAllText(fullFilePath, text);
                }
            }
        }
    }

    public static string GetUIdFromSceneFile(string path)
    {
        string uid;

        using StreamReader reader = new(path);

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

    public static string Highlight(string text)
    {
        return $"[wave amp=20.0 freq=2.0 connected=1][color=white]{text}[/color][/wave]";
    }

    public static string FormatGameName(string name)
    {
        return name.Trim().FirstCharToUpper().Replace(" ", "");
    }

    public static bool IsAlphaNumericAndAllowSpaces(string str)
    {
        return AlphaNumericAndSpacesRegex().IsMatch(str);
    }

    public static void DisplayGameNamePreview(string inputName, RichTextLabel gameNamePreview)
    {
        string name = FormatGameName(inputName);

        string text = $"The name of the project will be {Highlight(name)}. " +
              $"The root namespace for all scripts will be {Highlight(name)}. " +
              $"Please ensure the name is in PascalFormat.";

        gameNamePreview.Text = text;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9\s,]*$")]
    private static partial Regex AlphaNumericAndSpacesRegex();
}
