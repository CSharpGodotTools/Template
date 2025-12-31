using Godot;
using GodotUtils;
using GodotUtils.RegEx;
using System.IO;

namespace __TEMPLATE__.Setup;

public static class SetupUtils
{
    /// <summary>
    /// Verifies the game name is not using the raw template namespace and not any existing class name in the project.
    /// </summary>
    public static bool IsGameNameBad(string name)
    {
        const string RawTemplateNamespace = "__TEMPLATE__";

        // Prevent game name being the same as the reserved raw template namespace name
        if (name.Equals(RawTemplateNamespace, System.StringComparison.OrdinalIgnoreCase))
        {
            GD.PrintErr($"{RawTemplateNamespace} is a reserved name.");
            return false;
        }

        // Prevent game name being the same as an existing class name in the project
        if (SetupUtils.EqualsExistingClassName(name))
        {
            GD.PrintErr($"Namespace {name} is the same name as {name}.cs");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns whether or not <paramref name="name"/> is equal to an existing script name in the project.
    /// </summary>
    public static bool EqualsExistingClassName(string name)
    {
        bool namespaceSameAsClassName = false;

        DirectoryUtils.Traverse("res://", fullFilePath =>
        {
            if (Path.GetFileName(fullFilePath).Equals(name + ".cs"))
            {
                namespaceSameAsClassName = true;
                return true;
            }

            return false;
        });

        return namespaceSameAsClassName;
    }

    /// <summary>
    /// Changes the 'run/main_scene' to <paramref name="sceneName"/> in the project.godot file at <paramref name="path"/>.
    /// </summary>
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

    /// <summary>
    /// Renames 'project/assembly_name' and 'config/name' to <paramref name="name"/> in the project.godot at <paramref name="path"/>.
    /// </summary>
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

    /// <summary>
    /// Renames the .sln file at <paramref name="path"/> to <paramref name="name"/> and replaces all occurences of "Template" in the .sln with <paramref name="name"/>.
    /// </summary>
    private static void RenameSolutionFile(string path, string name)
    {
        string fullPath = Path.Combine(path, "Template.sln");
        string text = File.ReadAllText(fullPath);
        text = text.Replace("Template", name);
        File.Delete(fullPath);
        File.WriteAllText(Path.Combine(path, name + ".sln"), text);
    }

    /// <summary>
    /// Renames the .csproj file at <paramref name="path"/> to <paramref name="name"/> and replaces the RootNamespace option with <paramref name="name"/>.
    /// </summary>
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

        bool RenameNamespaces(string fullFilePath)
        {
            // Ignore these directories
            switch (Path.GetDirectoryName(fullFilePath))
            {
                case ".godot":
                case "GodotUtils":
                case "addons":
                    return false;
            }

            // Prevent modifying the currently executing setup script
            if (fullFilePath.EndsWith($"{nameof(SetupUI)}.cs"))
                return false;

            // Modify all scripts
            if (fullFilePath.EndsWith(".cs"))
            {
                string text = File.ReadAllText(fullFilePath);

                    text = text.Replace($"namespace {oldNamespaceName}", $"namespace {newNamespaceName}");
                    text = text.Replace($"using {oldNamespaceName}", $"using {newNamespaceName}");
                    text = text.Replace($"{oldNamespaceName}.", $"{newNamespaceName}.");

                    File.WriteAllText(fullFilePath, text);
                }
            }

            return true;
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
        return RegexUtils.AlphaNumericAndSpaces().IsMatch(str);
    }

    public static void DisplayGameNamePreview(string inputName, RichTextLabel gameNamePreview)
    {
        string name = FormatGameName(inputName);

        string text = $"[color=gray]The name of the project will be {Highlight(name)}. " +
              $"The root namespace for all scripts will be {Highlight(name)}. " +
              $"Please ensure the name is in PascalFormat.[/color]";

        gameNamePreview.Text = text;
    }
}
