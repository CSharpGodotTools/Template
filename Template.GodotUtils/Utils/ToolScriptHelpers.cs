using Godot;
using System.IO;

namespace GodotUtils.UI;

[Tool]
public partial class ToolScriptHelpers : Node
{
    /// <summary>
    /// Triggers removal of empty folders when toggled in the editor.
    /// </summary>
    [Export]
#pragma warning disable CA1822 // Mark members as static
    public bool RemoveEmptyFolders
    {
        get => false;
        set => DeleteEmptyFolders();
    }

    /// <summary>
    /// Triggers removal of orphaned .cs.uid files when toggled in the editor.
    /// </summary>
    [Export]
    public bool RemoveOrphanedCSUIDFiles
    {
        get => false;
        set => DeleteOrphanedCSUIDFiles();
    }
#pragma warning restore CA1822 // Mark members as static

    private static void DeleteEmptyFolders()
    {
        if (!IsEditorContext()) // Do not trigger on game build
            return;

        DirectoryUtils.DeleteEmptyDirectories("res://");

        GD.Print("Removed all empty folders from the project. Restart the game engine or wait some time to see the effect.");
    }

    private static void DeleteOrphanedCSUIDFiles()
    {
        if (!IsEditorContext())
            return;

        string projectPath = ProjectSettings.GlobalizePath("res://");
        int deletedCount = 0;

        foreach (string file in Directory.GetFiles(projectPath, "*.cs.uid", SearchOption.AllDirectories))
        {
            string directory = Path.GetDirectoryName(file) ?? string.Empty;

            // Derive the expected .cs file from the .cs.uid filename.
            string baseName = Path.GetFileNameWithoutExtension(file).Replace(".cs", "");
            string csFile = Path.Combine(directory, baseName + ".cs");

            if (!File.Exists(csFile))
            {
                File.Delete(file);
                deletedCount++;
            }
        }

        GD.Print($"Deleted {deletedCount} orphaned .cs.uid files.");
    }

    private static bool IsEditorContext()
    {
        return Engine.IsEditorHint();
    }
}
