using System;
using System.IO;

namespace Framework.Setup;

public static class NamespaceMigration
{
    public static void RenameTemplateNamespaces(string projectRoot, string newNamespaceName)
    {
        string[] scriptFiles = Directory.GetFiles(projectRoot, "*.cs", SearchOption.AllDirectories);

        foreach (string scriptFile in scriptFiles)
        {
            if (ShouldSkipFile(scriptFile))
            {
                continue;
            }

            string scriptText = File.ReadAllText(scriptFile);

            scriptText = scriptText.Replace(
                $"namespace {GameNameRules.ReservedRawTemplateNamespace}",
                $"namespace {newNamespaceName}");

            scriptText = scriptText.Replace(
                $"using {GameNameRules.ReservedRawTemplateNamespace}",
                $"using {newNamespaceName}");

            scriptText = scriptText.Replace(
                $"{GameNameRules.ReservedRawTemplateNamespace}.",
                $"{newNamespaceName}.");

            File.WriteAllText(scriptFile, scriptText);
        }
    }

    private static bool ShouldSkipFile(string filePath)
    {
        string normalizedPath = filePath.Replace('\\', '/');
        string lowerPath = normalizedPath.ToLowerInvariant();

        return lowerPath.Contains("/.godot/")
               || lowerPath.Contains("/addons/")
               || lowerPath.Contains("/godotutils/")
               || lowerPath.Contains("/mods/")
               || lowerPath.Contains("/framework/");
    }
}
