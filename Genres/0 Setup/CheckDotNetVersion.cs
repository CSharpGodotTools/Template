using Godot;
using System;
using System.Diagnostics;

namespace Template;

[Tool]
public partial class CheckDotNetVersion : Node
{
    public override void _Ready()
    {
        CheckDotNet();
    }

    private void CheckDotNet()
    {
        string dotnetVersion = GetDotNetVersion();

        if (dotnetVersion != null && CompareVersions(dotnetVersion, "8.0.400") < 0)
        {
            AcceptDialog dialog = new();
            dialog.DialogText = "Your .NET version is lower than 8.0.400. Please update your .NET SDK from https://dotnet.microsoft.com/en-us/download";
            dialog.Connect("confirmed", new Callable(this, nameof(OnDialogConfirmed)));
            AddChild(dialog);
            dialog.PopupCentered();
        }
    }

    private string GetDotNetVersion()
    {
        try
        {
            Process process = new();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "--version";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Trim();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private int CompareVersions(string version1, string version2)
    {
        string[] v1Parts = version1.Split('.');
        string[] v2Parts = version2.Split('.');

        int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);

        for (int i = 0; i < maxLength; i++)
        {
            int v1Part = i < v1Parts.Length ? int.Parse(v1Parts[i]) : 0;
            int v2Part = i < v2Parts.Length ? int.Parse(v2Parts[i]) : 0;

            if (v1Part < v2Part)
                return -1;
            if (v1Part > v2Part)
                return 1;
        }

        return 0;
    }

    private void OnDialogConfirmed()
    {
        OS.ShellOpen("https://dotnet.microsoft.com/en-us/download");
    }
}
