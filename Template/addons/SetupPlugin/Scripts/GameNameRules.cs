using Godot;
using System;
using System.IO;

namespace Framework.Setup;

public static class GameNameRules
{
    public const string ReservedRawTemplateNamespace = "__TEMPLATE__";

    public static bool TryValidateForSetup(string gameName, out string validationError)
    {
        string formattedGameName = FormatGameName(gameName);

        if (string.IsNullOrWhiteSpace(formattedGameName))
        {
            validationError = "The game name cannot be empty.";
            return false;
        }

        if (formattedGameName.Equals(ReservedRawTemplateNamespace, StringComparison.OrdinalIgnoreCase))
        {
            validationError = $"{ReservedRawTemplateNamespace} is a reserved name.";
            return false;
        }

        if (EqualsExistingClassName(formattedGameName))
        {
            validationError = $"Namespace {formattedGameName} is the same name as {formattedGameName}.cs";
            return false;
        }

        validationError = string.Empty;
        return true;
    }

    public static string FormatGameName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        string trimmedName = name.Trim().Replace(" ", string.Empty);
        if (trimmedName.Length == 0)
        {
            return string.Empty;
        }

        string firstCharacter = trimmedName.Substring(0, 1).ToUpperInvariant();
        if (trimmedName.Length == 1)
        {
            return firstCharacter;
        }

        return firstCharacter + trimmedName.Substring(1);
    }

    public static bool IsAlphaNumericAndAllowSpaces(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character) || character == ' ')
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool EqualsExistingClassName(string name)
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string[] scriptFiles = Directory.GetFiles(projectRoot, "*.cs", SearchOption.AllDirectories);

        foreach (string scriptFile in scriptFiles)
        {
            string fileName = Path.GetFileName(scriptFile);

            if (fileName.Equals(name + ".cs", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
