using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;
using GodotUtils;

namespace Template.UI;

[Tool]
public partial class ToolScriptHelpers : Node
{
    [Export] 
    public bool RemoveEmptyFolders
    {
        get => false;
        set => DeleteEmptyFolders();
    }

    [Export]
    public bool RetrieveCredits
    {
        get => false;
        set => FetchAllCredits();
    }

    private static void DeleteEmptyFolders()
    {
        if (!Engine.IsEditorHint()) // Do not trigger on game build
        {
            return;
        }

        DirectoryUtils.DeleteEmptyDirectories("res://");
        GD.Print("Removed all empty folders from the project");
    }

    private static void FetchAllCredits()
    {
        if (!Engine.IsEditorHint()) // Do not trigger on game build
        {
            return;
        }

        StringBuilder creditsBuilder = new();

        DirectoryUtils.Traverse("res://", ProcessCreditFile);

        string finalCredits = creditsBuilder.ToString();

        GD.Print(finalCredits);

        void ProcessCreditFile(string fullFilePath)
        {
            if (string.Equals(Path.GetFileName(fullFilePath), "Credit.txt", StringComparison.OrdinalIgnoreCase))
            {
                string folderParentName = new DirectoryInfo(Path.GetDirectoryName(fullFilePath)).Name;
                
                Dictionary<string, string> creditInfo = [];

                foreach (string line in File.ReadAllLines(ProjectSettings.GlobalizePath(fullFilePath)))
                {
                    string[] parts = line.Split(':', 2);

                    creditInfo[parts[0].Trim().ToLower()] = parts[1].Trim();
                }

                string authorOrArtist = creditInfo.GetValueOrDefault("artist") ?? creditInfo.GetValueOrDefault("author") ?? "Unknown";
                string source = creditInfo.GetValueOrDefault("source") ?? "Unknown";
                string license = creditInfo.GetValueOrDefault("license") ?? "Unknown";

                creditsBuilder.AppendLine($"{folderParentName} made by {authorOrArtist} LICENSED {license}: {source}");
            }
        }
    }
}
