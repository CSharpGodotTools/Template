using Godot;
using System;
using System.Diagnostics;

namespace __TEMPLATE__.Setup;

public class SetupEditor
{
    public static void Start()
    {
        OS.Execute(OS.GetExecutablePath(), ["--editor"]);
    }

    public static void Restart()
    {
        Quit();
        Start();
    }

    public static void Quit()
    {
        string[] names = ["redot", "godot"];

        foreach (Process process in Process.GetProcesses())
        {
            foreach (string name in names)
            {
                string winTitle = process.MainWindowTitle.ToLower();

                if (winTitle.Contains(name) && !winTitle.Contains("console"))
                {
                    process.Kill();
                    return;
                }
            }
        }
    }
}
