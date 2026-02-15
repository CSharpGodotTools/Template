using System.IO;

namespace Framework.Setup;

public static class ProjectFileRenamer
{
    private const string TemplateProjectName = "Template";

    public static void RenameTemplateProjectFiles(string projectRoot, string projectName)
    {
        RenameCSProjFile(projectRoot, projectName);
        RenameSolutionFile(projectRoot, projectName);
        RenameProjectGodotFile(projectRoot, projectName);
    }

    private static void RenameProjectGodotFile(string projectRoot, string projectName)
    {
        string projectFilePath = Path.Combine(projectRoot, "project.godot");
        RequireFile(projectFilePath);

        string projectText = File.ReadAllText(projectFilePath);

        projectText = projectText.Replace(
            $"project/assembly_name=\"{TemplateProjectName}\"",
            $"project/assembly_name=\"{projectName}\"");

        projectText = projectText.Replace(
            $"config/name=\"{TemplateProjectName}\"",
            $"config/name=\"{projectName}\"");

        File.WriteAllText(projectFilePath, projectText);
    }

    private static void RenameSolutionFile(string projectRoot, string projectName)
    {
        string solutionFilePath = Path.Combine(projectRoot, $"{TemplateProjectName}.sln");
        RequireFile(solutionFilePath);

        string solutionText = File.ReadAllText(solutionFilePath);
        solutionText = solutionText.Replace(TemplateProjectName, projectName);

        File.Delete(solutionFilePath);
        File.WriteAllText(Path.Combine(projectRoot, projectName + ".sln"), solutionText);
    }

    private static void RenameCSProjFile(string projectRoot, string projectName)
    {
        string csProjFilePath = Path.Combine(projectRoot, $"{TemplateProjectName}.csproj");
        RequireFile(csProjFilePath);

        string csProjText = File.ReadAllText(csProjFilePath);
        csProjText = csProjText.Replace(
            $"<RootNamespace>{TemplateProjectName}</RootNamespace>",
            $"<RootNamespace>{projectName}</RootNamespace>");

        File.Delete(csProjFilePath);
        File.WriteAllText(Path.Combine(projectRoot, projectName + ".csproj"), csProjText);
    }

    private static void RequireFile(string path)
    {
        if (File.Exists(path))
        {
            return;
        }

        throw new FileNotFoundException($"Missing setup artifact: {path}");
    }
}
