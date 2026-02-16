using System.IO;

namespace Framework.Setup;

public sealed class SetupRuntimeStateValidator
{
    private const string TemplateProjectName = "Template";

    private readonly string _projectRoot;
    private readonly string _mainScenesRoot;

    public SetupRuntimeStateValidator(string projectRoot, string mainScenesRoot)
    {
        _projectRoot = projectRoot;
        _mainScenesRoot = mainScenesRoot;
    }

    public bool TryValidate(out string failureReason)
    {
        if (string.IsNullOrWhiteSpace(_projectRoot) || !Directory.Exists(_projectRoot))
        {
            failureReason = "Project root could not be resolved.";
            return false;
        }

        if (!File.Exists(Path.Combine(_projectRoot, "project.godot")))
        {
            failureReason = "The project.godot file is missing.";
            return false;
        }

        if (!File.Exists(Path.Combine(_projectRoot, $"{TemplateProjectName}.csproj")))
        {
            failureReason = $"{TemplateProjectName}.csproj was not found.";
            return false;
        }

        if (!Directory.Exists(_mainScenesRoot))
        {
            failureReason = $"Setup templates directory was not found: {_mainScenesRoot}";
            return false;
        }

        string formattedName = GameNameRules.FormatGameName("Validation");
        bool validCharacters = GameNameRules.IsAlphaNumericAndAllowSpaces(formattedName);

        if (!validCharacters)
        {
            failureReason = "Internal setup validation failed for game-name rules.";
            return false;
        }

        failureReason = string.Empty;
        return true;
    }
}
