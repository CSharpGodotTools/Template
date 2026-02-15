using Godot;
using System.IO;

namespace Framework.Setup;

public sealed class ProjectSetupRunner
{
    private const string SetupPluginName = "SetupPlugin";
    private const string MainScenePath = "application/run/main_scene";

    private readonly string _projectRoot;
    private readonly string _mainScenesRoot;

    public ProjectSetupRunner(string projectRoot, string mainScenesRoot)
    {
        _projectRoot = projectRoot;
        _mainScenesRoot = mainScenesRoot;
    }

    public void Run(string formattedGameName, string projectType, string templateType)
    {
        SetupDirectoryMaintenance.DeleteEmptyDirectories(_projectRoot);

        string templateFolder = Path.Combine(_mainScenesRoot, projectType, templateType);
        CopyTemplateToProjectRoot(templateFolder);

        ProjectSettings.SetSetting(MainScenePath, "res://Level.tscn");
        ProjectSettings.Save();

        ProjectFileRenamer.RenameTemplateProjectFiles(_projectRoot, formattedGameName);
        NamespaceMigration.RenameTemplateNamespaces(_projectRoot, formattedGameName);
        SetupFileSystem.EnsureGDIgnoreFilesInGDUnitTestFolders(_projectRoot);

        EditorInterface.Singleton.SaveScene();
        DisableAndDeleteSetupPlugin();
        EditorInterface.Singleton.RestartEditor(save: false);
    }

    private void CopyTemplateToProjectRoot(string templateFolder)
    {
        if (!Directory.Exists(templateFolder))
        {
            throw new DirectoryNotFoundException($"Template folder does not exist: {templateFolder}");
        }

        string[] sourceDirectories = Directory.GetDirectories(templateFolder, "*", SearchOption.AllDirectories);
        foreach (string sourceDirectory in sourceDirectories)
        {
            string relativePath = Path.GetRelativePath(templateFolder, sourceDirectory);
            string destinationDirectory = Path.Combine(_projectRoot, relativePath);
            Directory.CreateDirectory(destinationDirectory);
        }

        string[] sourceFiles = Directory.GetFiles(templateFolder, "*", SearchOption.AllDirectories);
        foreach (string sourceFile in sourceFiles)
        {
            string relativePath = Path.GetRelativePath(templateFolder, sourceFile);
            string destinationFile = Path.Combine(_projectRoot, relativePath);
            string destinationDirectory = Path.GetDirectoryName(destinationFile);

            if (destinationDirectory != null)
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(sourceFile, destinationFile, overwrite: true);
        }
    }

    private void DisableAndDeleteSetupPlugin()
    {
        EditorInterface.Singleton.SetPluginEnabled(SetupPluginName, false);

        string setupPluginPath = Path.Combine(_projectRoot, "addons", SetupPluginName);
        if (!Directory.Exists(setupPluginPath))
        {
            return;
        }

        Directory.Delete(setupPluginPath, recursive: true);
    }
}
